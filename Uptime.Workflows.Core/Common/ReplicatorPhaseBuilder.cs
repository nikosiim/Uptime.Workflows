using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Common;

public class ReplicatorPhaseBuilder(Dictionary<string, ReplicatorPhaseConfiguration> phaseConfigurations)
    : IReplicatorPhaseBuilder
{
    public List<ReplicatorPhase> BuildPhases(IWorkflowPayload payload, WorkflowId workflowId)
    {
        var phases = new List<ReplicatorPhase>();

        foreach ((string phaseName, ReplicatorPhaseConfiguration config) in phaseConfigurations)
        {
            var phase = new ReplicatorPhase
            {
                PhaseName = phaseName,
                Type = config.ReplicatorType(payload),
                TaskData = config.ActivityData(payload, workflowId).ToList()
            };

            phases.Add(phase);
        }

        return phases;
    }
}