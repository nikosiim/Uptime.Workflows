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
/// 
/// Provides the common building blocks needed to run a workflow:
/// • State machine lifecycle (start, modify, cancel, complete).
/// • Persistence of state and context across API calls and restarts.
/// • Integration with task service and history service for tracking.
/// 
/// New developers: 
/// Inherit from <c>WorkflowBase&lt;TContext&gt;</c> when creating a new workflow
/// (e.g. ApprovalWorkflow, SigningWorkflow). Override the abstract methods to
/// define your workflow’s specific states, triggers, and activities.
/// 
/// Think of this class as the “engine” — it knows *how* to run a workflow,
/// while your derived class defines *what the workflow actually does*.
/// </summary>
public abstract class WorkflowBase<TContext>(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService,
    ILogger<WorkflowBase<TContext>> logger)
    : IWorkflowMachine, IWorkflow<TContext> where TContext : class, IWorkflowContext, new()
{
    #region Fields & Properties

    protected StateMachine<BaseState, WorkflowTrigger> Machine = null!;

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

    #region Public Methods

    public async Task<Result<Unit>> StartAsync(StartWorkflowPayload payload, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();
        try
        {
            InitializeWorkflowContext(payload);
            await RegisterWorkflowInstanceAsync(ct);

            InitializeStateMachine(BaseState.NotStarted, ct);
            await OnWorkflowActivatedAsync(ct);
            
            logger.LogStarted(WorkflowDefinition, WorkflowId, AssociationName);

            await historyService.CreateAsync(
                WorkflowId,
                WorkflowEventType.WorkflowStarted,
                payload.ExecutorSid,
                description: WorkflowStartedHistoryDescription,
                ct:ct
            );

            await Machine.FireAsync(WorkflowTrigger.Start);
            await OnWorkflowStartedAsync(ct);
            await SaveWorkflowStateAsync(ct);
        }
        catch (WorkflowValidationException vex)
        {
            await HandleWorkflowExceptionAsync(vex, WorkflowId, ct);
            return Result<Unit>.Failure(vex.Error, vex.Message);
        }
        catch (Exception ex)
        {
            await HandleWorkflowExceptionAsync(ex, WorkflowId, ct);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        return Result<Unit>.Success(new Unit());
    }
    
    /// <summary>
    /// Called when a running workflow is modified by the user or system.
    /// 
    /// • Lets you inspect the incoming <paramref name="payload"/> (new data).
    /// • Compare it with the current <see cref="WorkflowContext"/> state.
    /// • Apply any changes (e.g. add/remove approvers, update tasks).
    /// • Return true if changes were applied, false otherwise.
    /// 
    /// New developers: This is the main “hook” for supporting workflow
    /// modification after it has already started.
    /// </summary>
    public async Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        try
        {
            if (await OnWorkflowModifiedAsync(payload, ct))
            {
                await SaveWorkflowStateAsync(ct);
            }
            
            logger.LogModified(WorkflowDefinition, WorkflowId, AssociationName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to modify workflow with ID {WorkflowId}", WorkflowId.Value);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        return Result<Unit>.Success(new Unit());
    }

    /// <summary>
    /// Restores a workflow instance from the database back into memory.
    /// 
    /// • Deserializes the <see cref="WorkflowContext"/> from JSON storage.
    /// • Restores the workflow’s current state (phase) into the state machine.
    /// • Reattaches the workflow’s ID so subsequent operations can track it.
    /// 
    /// New developers: This is called when a workflow is “resumed” rather than
    /// started fresh (for example, after an API call or app restart). Without
    /// this, the workflow would lose its context and could not continue.
    /// </summary>
    public Result<Unit> Rehydrate(string storageJson, string phase, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        try
        {
            WorkflowContext = BaseWorkflowContext.Deserialize<TContext>(storageJson);
            InitializeStateMachine(BaseState.FromString(phase), ct);
            
            logger.LogRehydrated(WorkflowDefinition, WorkflowId, AssociationName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rehydrate workflow with ID {WorkflowId}", WorkflowId);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> CancelAsync(CancelWorkflowPayload payload, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
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
                ct:ct
            );

            await CancelAllTasksAsync(ct);
            await Machine.FireAsync(WorkflowTrigger.Cancel);
            await SaveWorkflowStateAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while cancelling the workflow.");
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        return Result<Unit>.Success(new Unit());
    }
    
    /// <summary>
    /// Triggers a workflow state transition using the provided trigger.
    /// 
    /// • Queues the trigger into the state machine safely (one at a time).
    /// • Executes any configured state transitions or entry/exit actions.
    /// • By default, commits the new workflow state to the database.
    /// 
    /// New developers: This is the method you call when something *happens*
    /// (like a task completed, approval given, etc.) and the workflow needs
    /// to move forward. It’s the engine that drives the state machine.
    /// </summary>
    public async Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken ct, bool autoCommit = true)
    {
        var transitionQueue = new StateTransitionQueue<BaseState, WorkflowTrigger>(Machine, logger);
        await transitionQueue.EnqueueTriggerAsync(trigger, ct);

        if (autoCommit)
        {
            await SaveWorkflowStateAsync(ct);
        }
    }

    public Result<string> GetModificationContext()
    {
        string modificationContext = OnWorkflowModification();
    
        if (string.IsNullOrWhiteSpace(modificationContext))
        {
            logger.LogWarning("No valid modification context available for workflow {WorkflowId}.", WorkflowId);
            return Result<string>.Failure(ErrorCode.Validation);
        }

        return Result<string>.Success(modificationContext);
    }

    #endregion

    #region Protected Methods
    
    protected string? AssociationName => WorkflowContext.GetAssociationName();
    protected string? WorkflowStartedHistoryDescription { get; set; } = "Workflow has been started.";
    protected string? WorkflowCompletedHistoryDescription { get; set; } = "Workflow has been completed.";
    
    protected virtual void InitializeWorkflowContext(StartWorkflowPayload payload)
    {
        if (payload.DocumentId == default)
            throw new WorkflowValidationException(ErrorCode.Validation, "DocumentId is required.");

        if (payload.WorkflowTemplateId == default)
            throw new WorkflowValidationException(ErrorCode.Validation, "WorkflowTemplateId is required.");
        
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
    
    protected virtual async Task SaveWorkflowStateAsync(CancellationToken ct)
    {
        await workflowService.UpdateStateAsync(WorkflowContext, Machine.State, ct);
    }

    protected virtual async Task CancelAllTasksAsync(CancellationToken ct)
    {
        await taskService.CancelActiveTasksAsync(WorkflowId, ct);
    }
    
    #endregion

    #region Abstract Methods 

    protected abstract Task OnWorkflowActivatedAsync(CancellationToken ct);
    protected abstract void ConfigureStateMachineAsync(CancellationToken ct);
    protected abstract IWorkflowDefinition WorkflowDefinition { get; }

    #endregion

    #region Private Methods

    private async Task RegisterWorkflowInstanceAsync(CancellationToken ct)
    {
        // To persist the workflow's initial state and get a database-assigned ID
        WorkflowId = await workflowService.CreateAsync(WorkflowContext, ct);
    }

    private void InitializeStateMachine(BaseState initialPhase, CancellationToken ct)
    {
        Machine = new StateMachine<BaseState, WorkflowTrigger>(initialPhase);
        ConfigureStateMachineAsync(ct);

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
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing workflow exception.");
        }
    }

    #endregion
}