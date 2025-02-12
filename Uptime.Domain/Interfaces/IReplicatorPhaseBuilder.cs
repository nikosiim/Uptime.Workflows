using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

/// <summary>
/// Defines a builder for creating replicator phases from a workflow payload.
/// </summary>
/// <typeparam name="TData">The type of data for each replicator phase.</typeparam>
public interface IReplicatorPhaseBuilder<TData>
{
    /// <summary>
    /// Builds a list of replicator phases based on the given payload and workflow ID.
    /// </summary>
    /// <param name="payload">The workflow payload containing task data.</param>
    /// <param name="workflowId">The workflow ID for contextual data lookup.</param>
    /// <returns>A list of replicator phases.</returns>
    List<ReplicatorPhase<TData>> BuildPhases(IWorkflowPayload payload, WorkflowId workflowId);
}