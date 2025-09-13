using Microsoft.Extensions.Logging;
using Stateless;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

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
    IPrincipalResolver principalResolver,
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
        get => WorkflowContext.GetWorkflowId();
        private set => WorkflowContext.SetWorkflowId(value);
    }

    #endregion

    #region Public Methods

    public async Task<Result<Unit>> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();
        try
        {
            await InitializeWorkflowContextAsync(payload, cancellationToken);
            InitializeStateMachine(BaseState.NotStarted, cancellationToken);
            
            await RegisterWorkflowInstanceAsync(cancellationToken);
            await OnWorkflowActivatedAsync(cancellationToken);
            
            logger.LogStarted(WorkflowDefinition, WorkflowId, AssociationName);

            await historyService.CreateAsync(
                WorkflowId,
                WorkflowEventType.WorkflowStarted,
                WorkflowContext.GetInitiatorId(),
                description: WorkflowStartedHistoryDescription,
                cancellationToken:cancellationToken
            );

            await Machine.FireAsync(WorkflowTrigger.Start);
            await OnWorkflowStartedAsync(cancellationToken);
            await SaveWorkflowStateAsync(cancellationToken);
        }
        catch (WorkflowValidationException vex)
        {
            await HandleWorkflowExceptionAsync(vex, WorkflowId, cancellationToken);
            return Result<Unit>.Failure(vex.Error, vex.Message);
        }
        catch (Exception ex)
        {
            await HandleWorkflowExceptionAsync(ex, WorkflowId, cancellationToken);
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
    public async Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        try
        {
            if (await OnWorkflowModifiedAsync(payload, cancellationToken))
            {
                await SaveWorkflowStateAsync(cancellationToken);
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
    public Result<Unit> Rehydrate(string storageJson, string phase, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        try
        {
            WorkflowContext = BaseWorkflowContext.Deserialize<TContext>(storageJson);
            InitializeStateMachine(BaseState.FromString(phase), cancellationToken);
            
            logger.LogRehydrated(WorkflowDefinition, WorkflowId, AssociationName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rehydrate workflow with ID {WorkflowId}", WorkflowId);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> CancelAsync(PrincipalId principalId, string? comment, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
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
                principalId,
                description: comment,
                cancellationToken:cancellationToken
            );

            await CancelAllTasksAsync(cancellationToken);
            await Machine.FireAsync(WorkflowTrigger.Cancel);
            await SaveWorkflowStateAsync(cancellationToken);
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
    public async Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken cancellationToken, bool autoCommit = true)
    {
        var transitionQueue = new StateTransitionQueue<BaseState, WorkflowTrigger>(Machine, logger);
        await transitionQueue.EnqueueTriggerAsync(trigger, cancellationToken);

        if (autoCommit)
        {
            await SaveWorkflowStateAsync(cancellationToken);
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
    
    protected virtual async Task InitializeWorkflowContextAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        if (payload.DocumentId == default)
            throw new WorkflowValidationException(ErrorCode.Validation, "DocumentId is required.");

        if (payload.WorkflowTemplateId == default)
            throw new WorkflowValidationException(ErrorCode.Validation, "WorkflowTemplateId is required.");

        if (string.IsNullOrWhiteSpace(payload.PrincipalSid))
            throw new WorkflowValidationException(ErrorCode.Validation, "PrincipalSid is required.");

        Principal principal = await principalResolver.ResolveBySidAsync(payload.PrincipalSid, cancellationToken)
                              ?? throw new WorkflowValidationException(ErrorCode.NotFound, $"Initiator SID '{payload.PrincipalSid}' not found.");
        
        WorkflowContext.Storage.MergeWith(payload.Storage);

        WorkflowContext.SetDocumentId(payload.DocumentId);
        WorkflowContext.SetWorkflowTemplateId(payload.WorkflowTemplateId);
        WorkflowContext.SetInitiator(principal, payload.PrincipalSid);
    }
    
    protected virtual Task OnWorkflowStartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual string OnWorkflowModification()
    {
        return string.Empty;
    }

    protected virtual Task<bool> OnWorkflowModifiedAsync(ModificationPayload payload, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    protected virtual Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        WorkflowContext.Outcome = WorkflowOutcome.Completed;
        return Task.CompletedTask;
    }

    protected virtual Task OnWorkflowCancelledAsync(CancellationToken cancellationToken)
    {
        WorkflowContext.Outcome = WorkflowOutcome.Cancelled;
        return Task.CompletedTask;
    }
    
    protected virtual async Task SaveWorkflowStateAsync(CancellationToken cancellationToken)
    {
        await workflowService.UpdateStateAsync(WorkflowContext, Machine.State, cancellationToken);
    }

    protected virtual async Task CancelAllTasksAsync(CancellationToken cancellationToken)
    {
        await taskService.CancelActiveTasksAsync(WorkflowId, cancellationToken);
    }
    
    #endregion

    #region Abstract Methods 

    protected abstract Task OnWorkflowActivatedAsync(CancellationToken cancellationToken);
    protected abstract void ConfigureStateMachineAsync(CancellationToken cancellationToken);
    protected abstract IWorkflowDefinition WorkflowDefinition { get; }

    #endregion

    #region Private Methods

    private async Task RegisterWorkflowInstanceAsync(CancellationToken cancellationToken)
    {
        // To persist the workflow's initial state and get a database-assigned ID
        WorkflowId = await workflowService.CreateAsync(WorkflowContext, cancellationToken);
    }

    private void InitializeStateMachine(BaseState initialPhase, CancellationToken cancellationToken)
    {
        Machine = new StateMachine<BaseState, WorkflowTrigger>(initialPhase);
        ConfigureStateMachineAsync(cancellationToken);

        Machine.OnTransitionCompletedAsync(async transition =>
        {
            if (transition.Destination.Equals(BaseState.Completed))
            {
                await OnWorkflowCompletedAsync(cancellationToken);
                await historyService.CreateAsync(
                    WorkflowId,
                    WorkflowEventType.WorkflowCompleted,
                    PrincipalId.System,
                    description: WorkflowCompletedHistoryDescription,
                    cancellationToken: cancellationToken
                );

                logger.LogCompleted(WorkflowDefinition, WorkflowId, AssociationName);
            }
            else if (transition.Destination.Equals(BaseState.Cancelled))
            {
                await OnWorkflowCancelledAsync(cancellationToken);
                await historyService.CreateAsync(
                    WorkflowId,
                    WorkflowEventType.WorkflowCancelled,
                    PrincipalId.System,
                    description: string.Empty,
                    cancellationToken: cancellationToken
                );

                logger.LogCancelled(WorkflowDefinition, WorkflowId, AssociationName);
            }
        });
    }

    private async Task HandleWorkflowExceptionAsync(Exception ex, WorkflowId workflowId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogError(ex, "An error occurred while starting the workflow.");
            
            await workflowService.MarkAsInvalidAsync(WorkflowId, cancellationToken);
            await taskService.CancelActiveTasksAsync(workflowId, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing workflow exception.");
        }
    }

    #endregion
}