using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TContext>(IWorkflowService workflowService, ILogger<WorkflowBase<TContext>> logger) 
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

        WorkflowId = await workflowService.CreateWorkflowInstanceAsync(payload);

        OnWorkflowActivated(payload);

        await Machine.FireAsync(WorkflowTrigger.Start);

        return await SaveWorkflowStateAsync();
    }

    public async Task<bool> RehydrateAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        return await _asyncLockHelper.ExecuteSynchronizedAsync(async () =>
        {
            WorkflowDto? workflowInstance = await workflowService.GetWorkflowInstanceAsync(workflowId);
            if (workflowInstance is null)
            {
                logger.LogWarning("Workflow instance {WorkflowId} was not found.", workflowId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(workflowInstance.InstanceDataJson))
            {
                logger.LogWarning("Workflow instance {WorkflowId} has empty or missing instance data JSON. Using a new {TContext} instance.", workflowId, typeof(TContext).Name);
                WorkflowContext = new TContext();
            }
            else
            {
                try
                {
                    WorkflowContext = JsonSerializer.Deserialize<TContext>(workflowInstance.InstanceDataJson) ?? new TContext();
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to deserialize workflow instance data for WorkflowId {WorkflowId}. Using a new {TContext} instance.", workflowId, typeof(TContext).Name);
                    WorkflowContext = new TContext();
                }
            }

            WorkflowId = workflowId;
            InitializeStateMachine(workflowInstance.Phase);

            return true;
        }, cancellationToken);
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
        await workflowService.UpdateWorkflowStateAsync(WorkflowId, Machine.CurrentState, WorkflowContext);
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