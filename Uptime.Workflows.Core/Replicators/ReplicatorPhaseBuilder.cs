using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

/// <summary>
/// Default implementation of <see cref="IReplicatorPhaseBuilder"/>.
/// <para>
/// <b>How does this work?</b><br/>
/// - Takes a set of phase configurations (typically defined per workflow type, e.g., ApprovalWorkflow).
/// - At runtime, for a specific workflow instance, it calls the config's delegates to figure out what tasks/items to create for each phase.
/// </para>
/// <para>
/// <b>Why do we use delegates here?</b><br/>
/// - This allows each phase to have custom logic for how tasks are generated, based on current workflow data (e.g., who should approve, who should sign).
/// - The delegates are typically defined in the workflow definition.
/// </para>
/// </summary>
public class ReplicatorPhaseBuilder(Dictionary<string, ReplicatorPhaseConfiguration> phaseConfigurations)
    : IReplicatorPhaseBuilder
{
    public List<ReplicatorPhase> BuildPhases(IWorkflowContext context)
    {
        var phases = new List<ReplicatorPhase>();

        foreach ((string phaseName, ReplicatorPhaseConfiguration config) in phaseConfigurations)
        {
            var phase = new ReplicatorPhase
            {
                PhaseName = phaseName,
                Type = config.ReplicatorType(context),
                TaskContext = config.ActivityData(context).ToList()
            };

            phases.Add(phase);
        }

        return phases;
    }
}