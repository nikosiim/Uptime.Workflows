namespace Uptime.Workflows.Api.Contracts;

public record WorkflowDefinitionResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public IReadOnlyList<string>? Actions { get; init; }
    public IReadOnlyList<PhaseResponse>? Phases { get; init; }
}

public record PhaseResponse
{
    public required string PhaseId { get; init; }
    public bool UpdateEnabled { get; init; }
    public bool SupportsSequential { get; init; }
    public bool SupportsParallel { get; init; }
    public List<string>? Actions { get; init; }
}