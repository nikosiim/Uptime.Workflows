namespace Uptime.Workflows.Core;

public class ReplicatorConfiguration
{
    public IReadOnlyList<PhaseActivity> PhaseActivities { get; init; } = Array.Empty<PhaseActivity>();
    public Dictionary<string, ReplicatorPhaseConfiguration> PhaseConfigurations { get; set; } = new();
}