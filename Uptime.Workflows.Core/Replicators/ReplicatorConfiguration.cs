namespace Uptime.Workflows.Core;

public class ReplicatorConfiguration
{
    public IReadOnlyList<PhaseActivity> PhaseActivities { get; init; } = [];
    public Dictionary<string, ReplicatorPhaseConfiguration> PhaseConfigurations { get; set; } = new();
}