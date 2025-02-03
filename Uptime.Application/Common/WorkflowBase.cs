using Stateless;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Shared.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TData> : IWorkflow<TData> where TData : IWorkflowContext, new()
{
    protected readonly StateMachine<WorkflowStatus, WorkflowTrigger> Machine;

    // The in-memory data object with any replicator info, counters, child status, etc.
    public TData WorkflowContext { get; protected set; }

    public WorkflowStatus CurrentState => Machine.State;

    protected WorkflowBase(WorkflowStatus workflowStatus, TData workflowContext)
    {
        Machine = new StateMachine<WorkflowStatus, WorkflowTrigger>(workflowStatus);
        WorkflowContext = workflowContext;

        InitializeMachine();
    }

    private void InitializeMachine()
    {
        OnWorkflowExecuted();
        ConfigureStateMachine();
    }

    // Fire an event on the state machine.
    public virtual async Task FireAsync(WorkflowTrigger trigger)
    {
        await Machine.FireAsync(trigger);
    }

    protected abstract void OnWorkflowExecuted();
    protected abstract void ConfigureStateMachine();
}