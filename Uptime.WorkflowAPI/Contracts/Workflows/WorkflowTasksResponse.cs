namespace Uptime.Workflows.Api.Contracts;

public record WorkflowTasksResponse
{
    public required Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public string DisplayStatus { get; init; } = null!;
    public int InternalStatus { get; init; }
    public string? Description { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? StorageJson { get; init; }
}