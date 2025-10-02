namespace Uptime.Client.Contracts;

public record WorkflowTasksResponse
{
    public required Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public required string DisplayStatus { get; init; }
    public int InternalStatus { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset DueDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? StorageJson { get; init; }
}