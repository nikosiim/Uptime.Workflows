using Microsoft.Extensions.Logging;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TContext>(
    IWorkflowStateRepository<TContext> stateRepository,
    IWorkflowPersistenceService persistenceService, 
    ILogger<WorkflowBase<TContext>> logger) 
    : IWorkflowMachine, IWorkflow<TContext> where TContext : class, IWorkflowContext, new()
{
    #region Fields & Properties

    protected IStateMachine<WorkflowPhase, WorkflowTrigger> Machine = null!;
    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();

    private readonly AsyncLockHelper _asyncLockHelper = new();

    #endregion

    #region Public Methods

    public async Task<WorkflowPhase> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken = default)
    {
        InitializeStateMachine(WorkflowPhase.NotStarted);

        WorkflowId = await persistenceService.CreateWorkflowInstanceAsync(payload);

        OnWorkflowActivated(payload);

        await Machine.FireAsync(WorkflowTrigger.Start);

        return await SaveWorkflowStateAsync();
    }

    public async Task<bool> RehydrateAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        WorkflowStateData<TContext>? stateData = await stateRepository.GetWorkflowStateAsync(workflowId);
        if (stateData == null)
            return false;

        WorkflowContext = stateData.Context;
        WorkflowId = workflowId;
        InitializeStateMachine(stateData.Phase);

        return true;
    }

    public async Task TriggerTransitionAsync(WorkflowTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default)
    {
        var transitionQueue = new StateTransitionQueue<WorkflowPhase, WorkflowTrigger>(Machine, logger);
        await transitionQueue.EnqueueTriggerAsync(trigger, cancellationToken);

        if (autoCommit)
        {
            await SaveWorkflowStateAsync();
        }
    }

    public async Task<WorkflowPhase> AlterTaskCoreAsync(IAlterTaskPayload payload)
    {
        if (!CanAlterTask())
            return WorkflowPhase.Completed;

        return await AlterTaskInternalAsync(payload);
    }

    #endregion

    #region Protected Methods
    
    protected virtual async Task<WorkflowPhase> SaveWorkflowStateAsync()
    {
        await persistenceService.UpdateWorkflowStateAsync(WorkflowId, Machine.CurrentState, WorkflowContext);
        return Machine.CurrentState;
    }
    
    protected virtual void UpdateReplicatorState(Guid taskGuid, bool isCompleted)
    {
        // Default implementation: does nothing
    }

    #endregion

    #region Abstract Methods 

    protected abstract void OnWorkflowActivated(IWorkflowPayload payload);
    protected abstract void ConfigureStateMachine();
    protected abstract Task<WorkflowPhase> AlterTaskInternalAsync(IAlterTaskPayload payload);

    #endregion

    #region Private Methods

    private void InitializeStateMachine(WorkflowPhase initialPhase)
    {
        Machine = StateMachineFactory.Create<WorkflowPhase, WorkflowTrigger>(initialPhase);
        ConfigureStateMachine();
    }

    private bool CanAlterTask()
    {
        if (Machine.CurrentState == WorkflowPhase.Completed)
        {
            logger.LogDebug("Workflow is already completed. No modifications allowed.");
            return false;
        }

        return true;
    }

    #endregion
}