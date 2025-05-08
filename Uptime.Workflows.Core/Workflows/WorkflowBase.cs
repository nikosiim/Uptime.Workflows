using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Stateless;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Services;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

public abstract class WorkflowBase<TContext>(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService, 
    ILogger<WorkflowBase<TContext>> logger)
    : IWorkflowMachine, IWorkflow<TContext> where TContext : class, IWorkflowContext, new()
{
    #region Fields & Properties

    protected StateMachine<BaseState, WorkflowTrigger> Machine = null!;
    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();

    #endregion

    #region Public Methods

    public async Task<Result<Unit>> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();
        
        try
        {
            WorkflowContext.Storage.MergeWith(payload.Storage);

            InitializeStateMachine(BaseState.NotStarted, cancellationToken);

            WorkflowId = await workflowService.CreateAsync(payload, cancellationToken);

            OnWorkflowActivatedAsync(payload, cancellationToken);
            
            logger.LogStarted(WorkflowDefinition, WorkflowId, AssociationName);

            await historyService.CreateAsync(
                WorkflowId,
                WorkflowEventType.WorkflowStarted,
                payload.Originator,
                description: WorkflowStartedHistoryDescription,
                cancellationToken:cancellationToken
            );

            await Machine.FireAsync(WorkflowTrigger.Start);
            await OnWorkflowStartedAsync(payload, cancellationToken);
            await SaveWorkflowStateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleWorkflowExceptionAsync(ex, WorkflowId, cancellationToken);
            return Result<Unit>.Failure("An error occurred while starting the workflow.");
        }

        return Result<Unit>.Success(new Unit());
    }
    
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
            return Result<Unit>.Failure("Workflow update failed");
        }

        return Result<Unit>.Success(new Unit());
    }

    public Result<Unit> RehydrateAsync(Workflow instance, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        try
        {
            WorkflowContext = WorkflowContextHelper.Deserialize<TContext>(instance.StorageJson);
            WorkflowId = (WorkflowId)instance.Id;

            InitializeStateMachine(BaseState.FromString(instance.Phase), cancellationToken);
            
            logger.LogRehydrated(WorkflowDefinition, WorkflowId, AssociationName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rehydrate workflow with ID {WorkflowId}", instance.Id);
            return Result<Unit>.Failure("Workflow reHydration failed");
        }

        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> CancelAsync(string executor, string comment, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        if (Machine.State.IsFinal())
        {
            logger.LogFinalStateCancellation(WorkflowDefinition, WorkflowId, Machine.State);

            return Result<Unit>.Failure("Workflow is already in final state");
        }

        try
        {
            await historyService.CreateAsync(
                WorkflowId,
                WorkflowEventType.WorkflowComment,
                executor,
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
            return Result<Unit>.Failure("An error occurred while cancelling the workflow");
        }

        return Result<Unit>.Success(new Unit());
    }
    
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
            return Result<string>.Failure("No modification context available.");
        }

        return Result<string>.Success(modificationContext);
    }

    #endregion

    #region Protected Methods

    protected virtual string? WorkflowStartedHistoryDescription { get; set; }
    protected virtual string? WorkflowCompletedHistoryDescription { get; set; }
    protected virtual string? AssociationName => WorkflowContext.Storage.GetValueOrDefault(WorkflowStorageKeys.AssociationName);

    protected virtual Task OnWorkflowStartedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
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
        await workflowService.UpdateStateAsync(WorkflowId, Machine.State, WorkflowContext, cancellationToken);
    }

    protected virtual async Task CancelAllTasksAsync(CancellationToken cancellationToken)
    {
        await taskService.CancelActiveTasksAsync(WorkflowId, cancellationToken);
    }
    
    #endregion

    #region Abstract Methods 

    protected abstract void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    protected abstract void ConfigureStateMachineAsync(CancellationToken cancellationToken);
    protected abstract IWorkflowDefinition WorkflowDefinition { get; }

    #endregion

    #region Private Methods

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
                    "System",
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
                    "System",
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