using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TContext>(IWorkflowService workflowService, ILogger<WorkflowBase<TContext>> logger) : IWorkflowMachine, IWorkflow<TContext>
    where TContext : class, IWorkflowContext, new()
{
    protected IStateMachine<WorkflowPhase, WorkflowTrigger> Machine = null!;

    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();

    public IWorkflowService WorkflowService { get; } = workflowService;
    
    private readonly AsyncLockHelper _asyncLockHelper = new();

    public virtual async Task<WorkflowPhase> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken = default)
    {
        InitializeStateMachine(WorkflowPhase.NotStarted);

        WorkflowId = await WorkflowService.CreateWorkflowInstanceAsync(payload);

        OnWorkflowActivated(payload);

        await Machine.FireAsync(WorkflowTrigger.Start);

        return await SaveWorkflowStateAsync();
    }
    
    public async Task FireAsync(WorkflowTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default)
    {
        var transitionQueue = new StateTransitionQueue<WorkflowPhase, WorkflowTrigger>(Machine, logger);
        await transitionQueue.EnqueueTriggerAsync(trigger, cancellationToken);

        if (autoCommit)
        {
            await SaveWorkflowStateAsync();
        }
    }

    public virtual async Task<bool> ReHydrateAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        return await _asyncLockHelper.ExecuteSynchronizedAsync(async () =>
        {
            WorkflowDto? workflowInstance = await WorkflowService.GetWorkflowInstanceAsync(workflowId);
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

    public async Task<WorkflowPhase> TryAlterTaskAsync(IAlterTaskPayload payload)
    {
        if (!CanAlterTaskAsync())
            return WorkflowPhase.Completed;

        return await AlterTaskInternalAsync(payload);
    }

    protected void InitializeStateMachine(WorkflowPhase initialPhase)
    {
        Machine = StateMachineFactory.Create<WorkflowPhase, WorkflowTrigger>(initialPhase);
        ConfigureStateMachine();
    }

    protected virtual void UpdateReplicatorState(Guid taskGuid, bool isCompleted)
    {
        // Default implementation: does nothing
    }

    protected virtual async Task<WorkflowPhase> SaveWorkflowStateAsync()
    {
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, Machine.CurrentState, WorkflowContext);
        return Machine.CurrentState;
    }

    protected abstract void OnWorkflowActivated(IWorkflowPayload payload);

    protected abstract void ConfigureStateMachine();

    protected abstract Task<WorkflowPhase> AlterTaskInternalAsync(IAlterTaskPayload payload);

    private bool CanAlterTaskAsync()
    {
        if (Machine.CurrentState == WorkflowPhase.Completed)
        {
            logger.LogDebug("Workflow is already completed. No modifications allowed.");
            return false;
        }

        return true;
    }
}