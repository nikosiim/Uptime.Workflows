﻿using Stateless;
using System.Text.Json;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TContext>(IWorkflowService workflowService)
    : IWorkflowMachine, IWorkflow<TContext> where TContext : class, IWorkflowContext, new()
{
    protected StateMachine<WorkflowPhase, WorkflowTrigger> Machine = null!;

    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();

    public IWorkflowService WorkflowService => workflowService;

    public async Task FireAsync(string phaseName, WorkflowTrigger trigger)
    {
        await Machine.FireAsync(trigger);
    }

    public virtual async Task<WorkflowPhase> StartAsync(IWorkflowPayload payload)
    {
        InitializeStateMachine(WorkflowPhase.NotStarted);
        WorkflowId = await WorkflowService.CreateWorkflowInstanceAsync(payload);

        OnWorkflowActivated(payload);
        await FireAsync(WorkflowTrigger.Start);
        await UpdateWorkflowStateAsync();

        return Machine.State;
    }

    public virtual async Task<bool> ReHydrateAsync(WorkflowId workflowId)
    {
        WorkflowDto? workflowInstance = await WorkflowService.GetWorkflowInstanceAsync(workflowId);
        if (workflowInstance is null) return false;

        WorkflowContext = JsonSerializer.Deserialize<TContext>(workflowInstance.InstanceDataJson ?? string.Empty) ?? new TContext();
        WorkflowId = workflowId;

        InitializeStateMachine(workflowInstance.Phase);
        return true;
    }

    public virtual async Task FireAsync(WorkflowTrigger trigger)
    {
        await Machine.FireAsync(trigger);
    }

    public async Task UpdateWorkflowStateAsync()
    {
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, Machine.State, WorkflowContext);
    }

    protected void InitializeStateMachine(WorkflowPhase initialPhase)
    {
        Machine = new StateMachine<WorkflowPhase, WorkflowTrigger>(initialPhase);
        ConfigureStateMachine();
    }

    protected virtual void UpdateReplicatorState(Guid taskGuid, bool isCompleted)
    {
        // Default implementation: does nothing
    }

    protected virtual async Task<WorkflowPhase> CommitWorkflowStateAsync()
    {
        await UpdateWorkflowStateAsync();
        return Machine.State;
    }

    protected abstract void OnWorkflowActivated(IWorkflowPayload payload);

    protected abstract void ConfigureStateMachine();
}