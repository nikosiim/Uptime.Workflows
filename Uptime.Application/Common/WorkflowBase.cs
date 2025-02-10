using System.Text.Json;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TContext>(IWorkflowService workflowService) : IWorkflowMachine, IWorkflow<TContext>
    where TContext : class, IWorkflowContext, new()
{
    protected IStateMachine<WorkflowPhase, WorkflowTrigger> Machine = null!;

    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();

    public IWorkflowService WorkflowService { get; } = workflowService;

    public virtual async Task<WorkflowPhase> StartAsync(IWorkflowPayload payload)
    {
        InitializeStateMachine(WorkflowPhase.NotStarted);
        WorkflowId = await WorkflowService.CreateWorkflowInstanceAsync(payload);

        OnWorkflowActivated(payload);
        await FireAsync(WorkflowTrigger.Start);
        await UpdateWorkflowStateAsync();

        return Machine.CurrentState;
    }

    public virtual async Task<bool> ReHydrateAsync(WorkflowId workflowId)
    {
        WorkflowDto? workflowInstance = await WorkflowService.GetWorkflowInstanceAsync(workflowId);
        if (workflowInstance is null)
            return false;

        WorkflowContext = JsonSerializer.Deserialize<TContext>(
            workflowInstance.InstanceDataJson ?? string.Empty) ?? new TContext();
        WorkflowId = workflowId;

        InitializeStateMachine(workflowInstance.Phase);
        return true;
    }

    public virtual async Task FireAsync(WorkflowTrigger trigger, bool autoCommit = true)
    {
        Console.WriteLine($"Triggering workflow state change: {Machine.CurrentState} -> {trigger}");

        await Machine.FireAsync(trigger);

        Console.WriteLine($"New workflow state: {Machine.CurrentState}");

        if (autoCommit)
        {
            await CommitWorkflowStateAsync();
        }
    }

    public async Task UpdateWorkflowStateAsync()
    {
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, Machine.CurrentState, WorkflowContext);
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

    protected virtual async Task<WorkflowPhase> CommitWorkflowStateAsync()
    {
        await UpdateWorkflowStateAsync();
        return Machine.CurrentState;
    }

    protected abstract void OnWorkflowActivated(IWorkflowPayload payload);

    protected abstract void ConfigureStateMachine();

    protected abstract Task<WorkflowPhase> AlterTaskInternalAsync(IAlterTaskPayload payload);

    private bool CanAlterTaskAsync()
    {
        if (Machine.CurrentState == WorkflowPhase.Completed)
        {
            Console.WriteLine("Workflow is already completed. No modifications allowed.");
            return false;
        }

        return true;
    }
}