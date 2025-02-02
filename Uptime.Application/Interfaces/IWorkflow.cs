using Uptime.Application.Enums;
using Uptime.Domain.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflow<out TData>
{
    /// <summary>
    /// The current state of the workflow (e.g. Draft, PendingApproval, etc.).
    /// </summary>
    WorkflowStatus CurrentState { get; }

    /// <summary>
    /// The in-memory data object with any replicator info, counters, child status, etc.
    /// </summary>
    TData WorkflowContext { get; }

    /// <summary>
    /// (Optional) An asynchronous version of Fire if needed.
    /// </summary>
    Task FireAsync(WorkflowTrigger trigger);
}