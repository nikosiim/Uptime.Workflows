using System.Text.Json;
using Stateless;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Application.Models.Common;
using Uptime.Application.Services;
using Uptime.Shared.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TData>(IWorkflowService workflowService)
    : IWorkflow<TData> where TData : IWorkflowContext, new()
{
    protected StateMachine<WorkflowStatus, WorkflowTrigger> Machine = null!;
    protected int WorkflowId;

    public TData WorkflowContext { get; private set; } = new();

    public IWorkflowService WorkflowService => workflowService;

    public virtual async Task<WorkflowStatus> StartAsync(WorkflowPayload payload)
    {
        Machine = new StateMachine<WorkflowStatus, WorkflowTrigger>(WorkflowStatus.NotStarted);

        WorkflowId = await WorkflowService.CreateWorkflowInstanceAsync(payload);
        
        OnWorkflowStartedAsync(WorkflowId, payload);
        ConfigureStateMachine();

        await FireAsync(WorkflowTrigger.Start);
        
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, WorkflowStatus.InProgress, WorkflowContext);

        return Machine.State;
    }

    public virtual async Task<bool> ReHydrateAsync(int workflowId)
    {
        // Retrieve workflow instance
        WorkflowDto? workflowInstance = await workflowService.GetWorkflowInstanceAsync(workflowId);
        if (workflowInstance == null)
            return false;

        // Deserialize the workflow context
        var workflowContext = JsonSerializer.Deserialize<TData>(workflowInstance.InstanceDataJson ?? string.Empty);
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

    protected virtual async Task<WorkflowStatus> CommitWorkflowUpdateAsync()
    {
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, Machine.State, WorkflowContext);

        return Machine.State;
    }

    protected abstract void OnWorkflowStartedAsync(int workflowId, WorkflowPayload payload);
    protected abstract void ConfigureStateMachine();
}