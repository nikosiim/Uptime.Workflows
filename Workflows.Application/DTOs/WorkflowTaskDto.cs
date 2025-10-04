using Workflows.Core.Enums;

namespace Workflows.Application.DTOs;

public record WorkflowTaskDto
{
    public required Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public string DisplayStatus { get; init; } = null!;
    public WorkflowTaskStatus InternalStatus { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public string? Document { get; init; }
    public int WorkflowId { get; init; }
}