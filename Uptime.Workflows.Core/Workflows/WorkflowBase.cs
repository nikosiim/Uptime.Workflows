using Microsoft.Extensions.Logging;
using Stateless;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

/// <summary>
/// Base class for all workflow implementations.
/// </summary>
/// <remarks>
/// <para><b>Lifecycle</b></para>
/// • <see cref="StartAsync"/> – initialize context, persist instance, and move from NotStarted to first state.  
/// • <see cref="Rehydrate"/> – restore context + state machine from DB.  
/// • <see cref="ModifyAsync"/> – apply mid-flight changes, then persist.  
/// • <see cref="TriggerTransitionAsync"/> – drive state changes when events happen.  
/// • <see cref="CancelAsync"/> – cancel workflow and active tasks.
///
/// <para><b>Usage notes (for new developers)</b></para>
/// • Put all long-form explanations here instead of on each method.
/// • See also: override points like <see cref="OnWorkflowActivatedAsync"/>,
///   <see cref="ConfigureStateMachineAsync"/>, and <see cref="BuildWorkflowStartedPayloadAsync"/>.
/// </remarks>
public abstract class WorkflowBase<TContext>(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService,
    IWorkflowOutboundNotifier? notifier,
    ILogger<WorkflowBase<TContext>> logger)
    : IWorkflowMachine, IWorkflow<TContext> where TContext : class, IWorkflowContext, new()
{
    #region Fields & Properties

    private readonly SemaphoreSlim _gate = new(1, 1);
    private StateTransitionQueue<BaseState, WorkflowTrigger>? _transitionQueue;

    /// <summary>
    /// Holds the runtime state (“memory”) of the workflow, including custom data defined
    /// by your workflow type. This context is:
    /// <list type="bullet">
    ///   <item><description>Created when the workflow starts (<c>new()</c> by default)</description></item>
    ///   <item><description>Continuously updated as the workflow progresses</description></item>
    ///   <item><description>Persisted as JSON and reloaded between API calls or restarts</description></item>
    /// </list>
    /// <para>
    /// <b>Tip:</b> Store here any data that must survive between transitions or restarts.
    /// </para>
    /// </summary>
    public TContext WorkflowContext { get; private set; } = new();

    protected StateMachine<BaseState, WorkflowTrigger> Machine = null!;
    protected IWorkflowOutboundNotifier? Notifier { get; } = notifier;
    protected string? AssociationName => WorkflowContext.GetAssociationName();
    protected string? WorkflowStartedHistoryDescription { get; set; } = "Workflow has been started.";
    protected string? WorkflowCompletedHistoryDescription { get; set; } = "Workflow has been completed.";

    protected WorkflowId WorkflowId
    {
        get
        {
            WorkflowId wrkId = WorkflowContext.GetWorkflowId();

            if (wrkId.Value == 0)
            {
                logger.LogError(
                    "[WorkflowId not initialized] Type: {Type} | Instance: {Hash} | Storage keys: {Keys} | WorkflowId in storage: {WorkflowIdValue}\nStack: {Stack}",
                    WorkflowContext.GetType().Name,
                    WorkflowContext.GetHashCode(),
                    string.Join(", ", WorkflowContext.Storage.Keys),
                    wrkId.Value,
                    Environment.StackTrace
                );
            }
            return WorkflowContext.GetWorkflowId();
        }
        private set => WorkflowContext.SetWorkflowId(value);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Initializes the workflow: creates context, registers the instance, 
    /// configures the state machine, and transitions to the first state.
    /// </summary>
    public Task<Result<Unit>> StartAsync(StartWorkflowPayload payload, CancellationToken ct)
        => ExecuteExclusiveAsync(async token =>
    {
        if (token.IsCancellationRequested) 
            return Result<Unit>.Cancelled();

        try
        {
            InitializeWorkflowContext(payload);
            await RegisterWorkflowInstanceAsync(token);

            InitializeStateMachine(BaseState.NotStarted, token);
            await OnWorkflowActivatedAsync(token);

            logger.LogStarted(WorkflowDefinition, WorkflowId, AssociationName);

            await historyService.CreateAsync(
                WorkflowId,
                WorkflowEventType.WorkflowStarted,
                payload.ExecutorSid,
                description: WorkflowStartedHistoryDescription,
                ct: token
            );

            await NotifyWorkflowStartedCoreAsync(token);

            await Machine.FireAsync(WorkflowTrigger.Start);
            await OnWorkflowStartedAsync(token);
            await SaveWorkflowStateAsync(token);
        }
        catch (WorkflowValidationException vex)
        {
            await HandleWorkflowExceptionAsync(vex, WorkflowId, token);
            return Result<Unit>.Failure(vex.Error, vex.Message);
        }
        catch (Exception ex)
        {
            await HandleWorkflowExceptionAsync(ex, WorkflowId, token);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        return Result<Unit>.Success(new Unit());
    }, ct);

    /// <summary>
    /// Applies a modification to a running workflow (compare payload to current <see cref="WorkflowContext"/>,
    /// update state/data, and persist changes).
    /// </summary>
    public Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken ct)
        => ExecuteExclusiveAsync(async token =>
    {
        if (token.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        try
        {
            if (await OnWorkflowModifiedAsync(payload, token))
            {
                await SaveWorkflowStateAsync(token);
            }

            logger.LogModified(WorkflowDefinition, WorkflowId, AssociationName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to modify workflow with ID {WorkflowId}", WorkflowId.Value);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        return Result<Unit>.Success(new Unit());
    }, ct);

    /// <summary>
    /// Restores <see cref="WorkflowContext"/> and the state machine from persisted storage.
    /// </summary>
    public Task<Result<Unit>> Rehydrate(string storageJson, string phase, CancellationToken ct)
        => ExecuteExclusiveAsync(token =>
    {
        if (token.IsCancellationRequested)
            return Task.FromResult(Result<Unit>.Cancelled());

        try
        {
            WorkflowContext = BaseWorkflowContext.Deserialize<TContext>(storageJson);
            InitializeStateMachine(BaseState.FromString(phase), token);

            logger.LogRehydrated(WorkflowDefinition, WorkflowId, AssociationName);

            return Task.FromResult(Result<Unit>.Success(new Unit()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rehydrate workflow with ID {WorkflowId}", WorkflowId);
            return Task.FromResult(Result<Unit>.Failure(ErrorCode.Unexpected));
        }
    }, ct);

    /// <summary>
    /// Cancels the workflow: transitions the state machine to <c>Cancelled</c>,
    /// invokes cleanup hooks, and persists the cancellation state.
    /// </summary>
    public Task<Result<Unit>> CancelAsync(CancelWorkflowPayload payload, CancellationToken ct)
        => ExecuteExclusiveAsync(async token =>
    {
        if (token.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        if (Machine.State.IsFinal())
        {
            logger.LogFinalStateCancellation(WorkflowDefinition, WorkflowId, Machine.State);
            return Result<Unit>.Failure(ErrorCode.Conflict);
        }

        try
        {
            await historyService.CreateAsync(
                WorkflowId,
                WorkflowEventType.WorkflowComment,
                payload.ExecutorSid,
                description: payload.Comment,
                ct: token
            );

            await CancelAllTasksAsync(token);
            await Machine.FireAsync(WorkflowTrigger.Cancel);
            await SaveWorkflowStateAsync(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while cancelling the workflow.");
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        return Result<Unit>.Success(new Unit());
    }, ct);
    
    /// <summary>
    /// Provides a workflow-specific modification context for clients.
    /// This is generated by <see cref="OnWorkflowModification"/> in the derived workflow
    /// (e.g. returning the current replicator state in an approval workflow),
    /// so that clients can decide which tasks to cancel, move, or add.
    /// </summary>
    public Result<string> GetModificationContext()
    {
        string modificationContext = OnWorkflowModification();
    
        if (string.IsNullOrWhiteSpace(modificationContext))
        {
            BaseState phase = Machine?.State ?? BaseState.NotStarted;
            logger.LogWarning("No valid modification context for {WorkflowId} in phase {Phase}.", WorkflowId, phase);

            return Result<string>.Failure(ErrorCode.Validation);
        }

        return Result<string>.Success(modificationContext);
    }

    #endregion

    #region Protected Hooks

    protected virtual void InitializeWorkflowContext(StartWorkflowPayload payload)
    {
        if (payload.DocumentId == default)
            throw new WorkflowValidationException(ErrorCode.Validation, "DocumentId is required.");

        if (payload.WorkflowTemplateId == default)
            throw new WorkflowValidationException(ErrorCode.Validation, "WorkflowTemplateId is required.");

        if (payload.ExecutorSid.Value is null || string.IsNullOrWhiteSpace(payload.ExecutorSid.Value))
            throw new WorkflowValidationException(ErrorCode.Validation, "Executor SID is required.");

        WorkflowContext.Storage.MergeWith(payload.Storage);

        WorkflowContext.SetDocumentId(payload.DocumentId);
        WorkflowContext.SetWorkflowTemplateId(payload.WorkflowTemplateId);
        WorkflowContext.SetInitiatorSid(payload.ExecutorSid);
    }
    protected virtual Task OnWorkflowStartedAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
    protected virtual string OnWorkflowModification()
    {
        return string.Empty;
    }
    protected virtual Task<bool> OnWorkflowModifiedAsync(ModificationPayload payload, CancellationToken ct)
    {
        return Task.FromResult(false);
    }
    protected virtual Task OnWorkflowCompletedAsync(CancellationToken ct)
    {
        WorkflowContext.Outcome = WorkflowOutcome.Completed;
        return Task.CompletedTask;
    }
    protected virtual Task OnWorkflowCancelledAsync(CancellationToken ct)
    {
        WorkflowContext.Outcome = WorkflowOutcome.Cancelled;
        return Task.CompletedTask;
    }
    protected virtual Task<WorkflowStartedPayload> BuildWorkflowStartedPayloadAsync(CancellationToken ct)
    {
        // Default: no assignees. Replicator base will override; simple workflows can override or leave empty.
        var payload = new WorkflowStartedPayload(
            WorkflowId: WorkflowId,
            WorkflowType: GetType().Name,
            StartedBySid: WorkflowContext.GetInitiatorSid(),
            Assignees: new List<AssigneeProjection>(),
            StartedAtUtc: DateTimeOffset.UtcNow);

        return Task.FromResult(payload);
    }
    protected virtual Task<WorkflowCompletedPayload> BuildWorkflowCompletedPayloadAsync(CancellationToken ct)
    {
        var payload = new WorkflowCompletedPayload(
            WorkflowId: WorkflowId,
            WorkflowType: GetType().Name,
            Outcome: WorkflowContext.Outcome,
            CompletedAtUtc: DateTimeOffset.UtcNow);
        return Task.FromResult(payload);
    }

    #endregion

    #region Protected Internals (non-overridable)
    
    protected internal Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken ct, bool autoCommit = true)
    {
        return _transitionQueue!.EnqueueTriggerAsync(trigger, ct)
            .ContinueWith(async _ => { if (autoCommit) await SaveWorkflowStateAsync(ct); }, ct,
                TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default).Unwrap();
    }
    protected async Task SaveWorkflowStateAsync(CancellationToken ct)
    {
        await workflowService.UpdateStateAsync(WorkflowContext, Machine.State, ct);
    }
    protected async Task CancelAllTasksAsync(CancellationToken ct)
    {
        await taskService.CancelActiveTasksAsync(WorkflowId, ct);
    }
    protected async Task NotifyWorkflowStartedCoreAsync(CancellationToken ct)
    {
        if (Notifier is null) return;

        WorkflowStartedPayload payload = await BuildWorkflowStartedPayloadAsync(ct);
        await Notifier.NotifyWorkflowStartedAsync(payload, ct);
    }
    protected async Task NotifyWorkflowCompletedCoreAsync(CancellationToken ct)
    {
        if (Notifier is null) return;

        WorkflowCompletedPayload payload = await BuildWorkflowCompletedPayloadAsync(ct);
        await Notifier.NotifyWorkflowCompletedAsync(payload, ct);
    }

    #endregion

    #region Abstract Members
    protected abstract IWorkflowDefinition WorkflowDefinition { get; }
    protected abstract Task OnWorkflowActivatedAsync(CancellationToken ct);
    protected abstract void ConfigureStateMachineAsync(CancellationToken ct);

    #endregion

    #region Private Internals

    private async Task<T> ExecuteExclusiveAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try { return await action(ct); }
        finally { _gate.Release(); }
    }
    private async Task RegisterWorkflowInstanceAsync(CancellationToken ct)
    {
        // To persist the workflow's initial state and get a database-assigned ID
        WorkflowId = await workflowService.CreateAsync(WorkflowContext, ct);
    }
    private void InitializeStateMachine(BaseState initialPhase, CancellationToken ct)
    {
        Machine = new StateMachine<BaseState, WorkflowTrigger>(initialPhase);
        ConfigureStateMachineAsync(ct);

        _transitionQueue = new StateTransitionQueue<BaseState, WorkflowTrigger>(Machine, logger);

        Machine.OnTransitionCompletedAsync(async transition =>
        {
            if (transition.Destination.Equals(BaseState.Completed))
            {
                await OnWorkflowCompletedAsync(ct);
                await historyService.CreateAsync(
                    WorkflowId,
                    WorkflowEventType.WorkflowCompleted,
                    Principal.SystemSid,
                    description: WorkflowCompletedHistoryDescription,
                    ct: ct
                );

                await NotifyWorkflowCompletedCoreAsync(ct);

                logger.LogCompleted(WorkflowDefinition, WorkflowId, AssociationName);
            }
            else if (transition.Destination.Equals(BaseState.Cancelled))
            {
                await OnWorkflowCancelledAsync(ct);
                await historyService.CreateAsync(
                    WorkflowId,
                    WorkflowEventType.WorkflowCancelled,
                    Principal.SystemSid,
                    description: string.Empty,
                    ct: ct
                );

                logger.LogCancelled(WorkflowDefinition, WorkflowId, AssociationName);
            }
        });
    }
    private async Task HandleWorkflowExceptionAsync(Exception ex, WorkflowId workflowId, CancellationToken ct)
    {
        try
        {
            logger.LogError(ex, "An error occurred while starting the workflow.");
            
            await workflowService.MarkAsInvalidAsync(WorkflowId, ct);
            await taskService.CancelActiveTasksAsync(workflowId, ct);

            await historyService.CreateAsync(
                workflowId,
                WorkflowEventType.WorkflowComment,
                Principal.SystemSid,
                description: $"Workflow marked invalid due to error: {ex.GetType().Name}",
                ct: ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing workflow exception.");
        }
    }

    #endregion
}