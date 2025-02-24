namespace Uptime.Domain.Common;

public class WorkflowConfiguration
{
    public List<PhaseConfiguration> Phases { get; set; } = [];
    public List<OutcomeConfiguration> Outcomes { get; set; } = [];
    public Dictionary<string, ReplicatorPhaseConfiguration> ReplicatorPhaseConfigurations { get; set; } = new();
}

public class PhaseConfiguration
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public bool SupportsSequential { get; set; }
    public bool SupportsParallel { get; set; }
    public string[] Actions { get; set; } = [];
}

public class OutcomeConfiguration
{
    public required string Key { get; set; }
    public required string DisplayValue { get; set; }
}