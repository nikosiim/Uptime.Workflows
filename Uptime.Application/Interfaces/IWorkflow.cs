namespace Uptime.Application.Interfaces;

public interface IWorkflow<out TData>
{
    /// <summary>
    /// The in-memory data object with any replicator info, counters, child status, etc.
    /// </summary>
    TData WorkflowContext { get; }
}