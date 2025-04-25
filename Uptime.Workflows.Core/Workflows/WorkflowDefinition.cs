namespace Uptime.Workflows.Core;

public record WorkflowDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public IReadOnlyList<string>? Actions { get; init; }
    public IReadOnlyList<PhaseActivity>? ReplicatorActivities { get; init; }
}

public record PhaseActivity
{
    public required string PhaseId { get; init; }
    public bool UpdateEnabled { get; init; }
    public bool SupportsSequential { get; init; }
    public bool SupportsParallel { get; init; }
    public required IReadOnlyList<string> Actions { get; init; }
}