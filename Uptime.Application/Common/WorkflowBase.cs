using Stateless;
using System.Text.Json;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TContext>(IWorkflowService workflowService)
    : IWorkflow<TContext> where TContext : IWorkflowContext, new()
{
    protected StateMachine<WorkflowStatus, WorkflowTrigger> Machine = null!;

    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();

    public IWorkflowService WorkflowService => workflowService;

    public virtual async Task<WorkflowStatus> StartAsync(IWorkflowPayload payload)
    {
        InitializeStateMachine(WorkflowStatus.NotStarted);
        WorkflowId = await WorkflowService.CreateWorkflowInstanceAsync(payload);

        OnWorkflowActivated(WorkflowId, payload);
        await FireAsync(WorkflowTrigger.Start);
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, WorkflowStatus.InProgress, WorkflowContext);

        return Machine.State;
    }

    public virtual async Task<bool> ReHydrateAsync(WorkflowId workflowId)
    {
        var workflowInstance = await WorkflowService.GetWorkflowInstanceAsync(workflowId);
        if (workflowInstance is null) return false;

        WorkflowContext = JsonSerializer.Deserialize<TContext>(workflowInstance.InstanceDataJson ?? string.Empty) ?? new TContext();
        WorkflowId = workflowId;

        InitializeStateMachine(workflowInstance.Status);
        return true;
    }

    public virtual async Task FireAsync(WorkflowTrigger trigger)
    {
        await Machine.FireAsync(trigger);
    }

    protected void InitializeStateMachine(WorkflowStatus initialState)
    {
        Machine = new StateMachine<WorkflowStatus, WorkflowTrigger>(initialState);
        ConfigureStateMachine();
    }

    protected virtual async Task<WorkflowStatus> CommitWorkflowStateAsync()
    {
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, Machine.State, WorkflowContext);
        return Machine.State;
    }

    protected abstract void OnWorkflowActivated(WorkflowId workflowId, IWorkflowPayload payload);

    protected abstract void ConfigureStateMachine();
}