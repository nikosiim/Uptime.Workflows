using Uptime.Application.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflow<out TData>
{
    /// <summary>
    /// The in-memory data object with any replicator info, counters, child status, etc.
    /// </summary>
    TData WorkflowContext { get; }

    /// <summary>
    /// (Optional) An asynchronous version of Fire if needed.
    /// </summary>
    Task FireAsync(WorkflowTrigger trigger);
}