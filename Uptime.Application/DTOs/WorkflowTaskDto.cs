using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Application.DTOs;

public record WorkflowTaskDto
{
    public int Id { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public string DisplayStatus { get; init; } = null!;
    public WorkflowTaskStatus InternalStatus { get; init; }
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public string? Document { get; init; }
    public int WorkflowId { get; init; }
}