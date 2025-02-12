using Microsoft.Extensions.Logging;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TContext>(IWorkflowStateRepository<TContext> stateRepository, ILogger<WorkflowBase<TContext>> logger) 
    : IWorkflowMachine, IWorkflow<TContext> where TContext : class, IWorkflowContext, new()
{
    #region Fields & Properties

    protected IStateMachine<WorkflowPhase, WorkflowTrigger> Machine = null!;
    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();
    public WorkflowPhase CurrentState => Machine.CurrentState;
    
    #endregion

    #region Public Methods
    
    public async Task<WorkflowPhase> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken = default)
    {
        InitializeStateMachine(WorkflowPhase.NotStarted);

        WorkflowId = await stateRepository.CreateWorkflowStateAsync(payload);

        OnWorkflowActivated(payload);

        await Machine.FireAsync(WorkflowTrigger.Start);
        await SaveWorkflowStateAsync();

        return Machine.CurrentState;
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
    
    #endregion

    #region Protected Methods
    
    protected virtual async Task SaveWorkflowStateAsync()
    {
        await stateRepository.SaveWorkflowStateAsync(WorkflowId, Machine.CurrentState, WorkflowContext);
    }

    #endregion

    #region Abstract Methods 

    protected abstract void OnWorkflowActivated(IWorkflowPayload payload);
    protected abstract void ConfigureStateMachine();

    #endregion

    #region Private Methods

    private void InitializeStateMachine(WorkflowPhase initialPhase)
    {
        Machine = StateMachineFactory.Create<WorkflowPhase, WorkflowTrigger>(initialPhase);
        ConfigureStateMachine();
    }
    
    #endregion
}