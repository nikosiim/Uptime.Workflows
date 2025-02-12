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
        InitializeStateMachineAsync(WorkflowPhase.NotStarted, cancellationToken);

        WorkflowId = await repository.CreateWorkflowInstanceAsync(payload, cancellationToken);

        OnWorkflowActivatedAsync(payload, cancellationToken);

        await Machine.FireAsync(WorkflowTrigger.Start);
        await SaveWorkflowStateAsync(cancellationToken);

        return Machine.CurrentState;
    }

    public async Task<bool> RehydrateAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        Workflow? instance = await repository.GetWorkflowInstanceAsync(workflowId, cancellationToken);
        if (instance == null)
            return false;

        WorkflowContext = WorkflowContextHelper.Deserialize<TContext>(instance.StorageJson);
        WorkflowId = workflowId;

        InitializeStateMachineAsync(instance.Phase, cancellationToken);

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
    
    protected virtual async Task SaveWorkflowStateAsync(CancellationToken cancellationToken)
    {
        await repository.SaveWorkflowStateAsync(WorkflowId, Machine.CurrentState, WorkflowContext, cancellationToken);
    }

    #endregion

    #region Abstract Methods 

    protected abstract void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    protected abstract void ConfigureStateMachineAsync(CancellationToken cancellationToken);

    #endregion

    #region Private Methods

    private void InitializeStateMachineAsync(WorkflowPhase initialPhase, CancellationToken cancellationToken)
    {
        Machine = stateMachineFactory.Create(initialPhase);
        ConfigureStateMachineAsync(cancellationToken);
    }

    #endregion
}