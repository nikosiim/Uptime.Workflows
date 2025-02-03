using Uptime.Shared.Enums;

namespace Uptime.Shared.Models.Workflows;

public record WorkflowTasksResponse
{
    public int Id { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public WorkflowTaskStatus Status { get; init; }
    public string? Description { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? StorageJson { get; init; }
}