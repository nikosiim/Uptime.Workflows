using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public class ReplicatorPhaseBuilder(Dictionary<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<object>>> phaseConfigurations)
    : IReplicatorPhaseBuilder
{
    /// <summary>
    /// Builds the replicator phases using the configured phase names and task data extraction functions.
    /// </summary>
    public List<ReplicatorPhase> BuildPhases(IWorkflowPayload payload, WorkflowId workflowId)
    {
        var phases = new List<ReplicatorPhase>();
        foreach (KeyValuePair<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<object>>> kvp in phaseConfigurations)
        {
            // Create a replicator phase for each configured phase.
            var phase = new ReplicatorPhase
            {
                PhaseName = kvp.Key,
                TaskData = kvp.Value(payload, workflowId).ToList()
            };
            phases.Add(phase);
        }
        return phases;
    }
}