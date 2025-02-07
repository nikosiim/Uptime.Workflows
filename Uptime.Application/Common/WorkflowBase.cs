using Stateless;
using System.Text.Json;
using Uptime.Application.DTOs;
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
        Machine = new StateMachine<WorkflowStatus, WorkflowTrigger>(WorkflowStatus.NotStarted);

        WorkflowId = await WorkflowService.CreateWorkflowInstanceAsync(payload);

        OnWorkflowActivated(WorkflowId, payload);
        ConfigureStateMachine();

        await FireAsync(WorkflowTrigger.Start);

        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, WorkflowStatus.InProgress, WorkflowContext);

        return Machine.State;
    }

    public virtual async Task<bool> ReHydrateAsync(WorkflowId workflowId)
    {
        // Retrieve workflow instance
        WorkflowDto? workflowInstance = await workflowService.GetWorkflowInstanceAsync(workflowId);
        if (workflowInstance == null)
            return false;

        // Deserialize the workflow context
        var workflowContext = JsonSerializer.Deserialize<TContext>(workflowInstance.InstanceDataJson ?? string.Empty);
        if (workflowContext == null)
            return false;

        WorkflowContext = workflowContext;
        Machine = new StateMachine<WorkflowStatus, WorkflowTrigger>(workflowInstance.Status);
        WorkflowId = workflowId;

        ConfigureStateMachine();

        return true;
    }

    public virtual async Task FireAsync(WorkflowTrigger trigger)
    {
        await Machine.FireAsync(trigger);
    }

    protected virtual async Task<WorkflowStatus> CommitWorkflowStateAsync()
    {
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, Machine.State, WorkflowContext);

        return Machine.State;
    }

    protected abstract void OnWorkflowActivated(WorkflowId workflowId, IWorkflowPayload payload);

    protected abstract void ConfigureStateMachine();
}