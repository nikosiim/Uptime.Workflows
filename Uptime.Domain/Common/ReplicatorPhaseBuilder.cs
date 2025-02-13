using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

/// <summary>
/// A default implementation of <see cref="IReplicatorPhaseBuilder{TData}"/> that builds replicator phases
/// based on configured phase names and associated functions to extract task data from the payload.
/// </summary>
/// <typeparam name="TData">The type of data for each replicator phase.</typeparam>
public class ReplicatorPhaseBuilder<TData>(Dictionary<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<TData>>> phaseConfigurations)
    : IReplicatorPhaseBuilder<TData>
{
    /// <summary>
    /// Builds the replicator phases using the configured phase names and task data extraction functions.
    /// </summary>
    public List<ReplicatorPhase<TData>> BuildPhases(IWorkflowPayload payload, WorkflowId workflowId)
    {
        var phases = new List<ReplicatorPhase<TData>>();
        foreach (KeyValuePair<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<TData>>> kvp in phaseConfigurations)
        {
            // Create a replicator phase for each configured phase.
            var phase = new ReplicatorPhase<TData>
            {
                PhaseName = kvp.Key,
                TaskData = kvp.Value(payload, workflowId).ToList()
            };
            phases.Add(phase);
        }
        return phases;
    }
}