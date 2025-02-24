using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class WorkflowBase<TContext>(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory, 
    IWorkflowRepository repository, 
    ILogger<WorkflowBase<TContext>> logger)
    : IWorkflowMachine, IWorkflow<TContext> where TContext : class, IWorkflowContext, new()
{
    #region Fields & Properties

    protected IStateMachine<WorkflowPhase, WorkflowTrigger> Machine = null!;
    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();
    public WorkflowPhase CurrentState => Machine.CurrentState;

    #endregion

    #region Public Methods

    public async Task<WorkflowPhase> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        WorkflowContext.Storage.MergeWith(payload.Storage);

        InitializeStateMachine(WorkflowPhase.NotStarted, cancellationToken);

        WorkflowId = await repository.CreateWorkflowInstanceAsync(payload, cancellationToken);

        try
        {
            OnWorkflowActivatedAsync(payload, cancellationToken);

            await repository.AddWorkflowHistoryAsync(
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
        }

        return Machine.CurrentState;
    }

    public async Task CancelWorkflowAsync(CancellationToken cancellationToken)
    {
        if (Machine.CurrentState.IsFinal())
        {
            logger.LogInformation("Workflow {WorkflowId} is already in final state '{State}'; no cancellation needed.", WorkflowId, CurrentState);
            return;
        }

        await CancelAllTasksAsync(cancellationToken);

        try
        {
            await Machine.FireAsync(WorkflowTrigger.Cancel);
            await SaveWorkflowStateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while cancelling the workflow.");
            throw;
        }
    }

    public async Task<bool> RehydrateAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        Workflow? instance = await repository.GetWorkflowInstanceAsync(workflowId, cancellationToken);
        if (instance == null)
            return false;

        WorkflowContext = WorkflowContextHelper.Deserialize<TContext>(instance.StorageJson);
        WorkflowId = workflowId;
        
        InitializeStateMachine(WorkflowPhase.FromString(instance.Phase), cancellationToken);

        return true;
    }

    public async Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken cancellationToken, bool autoCommit = true)
    {
        var transitionQueue = new StateTransitionQueue<WorkflowPhase, WorkflowTrigger>(Machine, logger);
        await transitionQueue.EnqueueTriggerAsync(trigger, cancellationToken);

        if (autoCommit)
        {
            await SaveWorkflowStateAsync(cancellationToken);
        }
    }

    #endregion

    #region Protected Methods

    protected virtual string? WorkflowStartedHistoryDescription { get; set; }
    protected virtual string? WorkflowCompletedHistoryDescription { get; set; }

    protected virtual Task OnWorkflowStartedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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
        await repository.SaveWorkflowStateAsync(WorkflowId, Machine.CurrentState, WorkflowContext, cancellationToken);
    }

    protected virtual async Task CancelAllTasksAsync(CancellationToken cancellationToken)
    {
        await repository.CancelAllActiveTasksAsync(WorkflowId, cancellationToken);
    }
    
    #endregion

    #region Abstract Methods 

    protected abstract void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    protected abstract void ConfigureStateMachineAsync(CancellationToken cancellationToken);
    protected abstract IWorkflowDefinition WorkflowDefinition { get; }

    #endregion

    #region Private Methods

    private void InitializeStateMachine(WorkflowPhase initialPhase, CancellationToken cancellationToken)
    {
        Machine = stateMachineFactory.Create(initialPhase);
        ConfigureStateMachineAsync(cancellationToken);

        Machine.OnTransitionCompletedAsync(async transition =>
        {
            if (transition.Destination == WorkflowPhase.Completed)
            {
                await OnWorkflowCompletedAsync(cancellationToken);
                await repository.AddWorkflowHistoryAsync(
                    WorkflowId,
                    WorkflowEventType.WorkflowCompleted,
                    "System",
                    description: WorkflowCompletedHistoryDescription,
                    cancellationToken: cancellationToken
                );
            }
            else if (transition.Destination == WorkflowPhase.Cancelled)
            {
                await OnWorkflowCancelledAsync(cancellationToken);
                await repository.AddWorkflowHistoryAsync(
                    WorkflowId,
                    WorkflowEventType.WorkflowCancelled,
                    "System",
                    description: string.Empty,
                    cancellationToken: cancellationToken
                );
            }
        });
    }

    private async Task HandleWorkflowExceptionAsync(Exception ex, WorkflowId workflowId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogError(ex, "An error occurred while starting the workflow.");
            
            await repository.MarkWorkflowAsInvalidAsync(WorkflowId, cancellationToken);
            await repository.CancelAllActiveTasksAsync(workflowId, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing workflow exception.");
        }
    }

    #endregion
}