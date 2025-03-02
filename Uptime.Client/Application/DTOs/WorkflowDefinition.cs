namespace Uptime.Client.Application.DTOs;

public class WorkflowDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public IReadOnlyList<string>? Actions { get; init; }
    public IReadOnlyList<PhaseActivity>? ReplicatorActivities { get; init; }
    public FormsConfiguration? FormsConfiguration { get; init; }
}

public record PhaseActivity
{
    public required string PhaseId { get; init; }
    public bool UpdateEnabled { get; init; }
    public bool SupportsSequential { get; init; }
    public bool SupportsParallel { get; init; }
    public List<string>? Actions { get; init; }
}

public record FormsConfiguration
{
    public required string Id { get; init; }
    public required string InitiationPage { get; init; }
    public required Type AssociationDialogType { get; init; }
    public required Type? ModificationDialogType { get; init; }
}