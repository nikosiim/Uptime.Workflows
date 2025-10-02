namespace Uptime.Workflows.Api.Contracts;

public record WorkflowTaskResponse
{
    public required Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public string Status { get; init; } = null!;
    public int InternalStatus { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public int DocumentId { get; init; }
    public int WorkflowId { get; init; }
    public string? PhaseId { get; init; }
    public string WorkflowBaseId { get; init; } = null!;
}