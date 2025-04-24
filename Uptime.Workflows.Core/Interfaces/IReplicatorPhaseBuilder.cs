using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

/// <summary>
/// Defines a builder for creating replicator phases from a workflow payload.
/// </summary>
public interface IReplicatorPhaseBuilder
{
    /// <summary>
    /// Builds a list of replicator phases based on the given payload and workflow ID.
    /// </summary>
    /// <param name="payload">The workflow payload containing task data.</param>
    /// <param name="workflowId">The workflow ID for contextual data lookup.</param>
    /// <returns>A list of replicator phases.</returns>
    List<ReplicatorPhase> BuildPhases(IWorkflowPayload payload, WorkflowId workflowId);
}