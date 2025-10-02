using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Application.DTOs;

public record DocumentWorkflowTaskDto
{
    public int TaskId { get; init; }
    public int WorkflowId { get; init; }
    public string? AssignedTo { get; init; }
    public string Status { get; init; } = null!;
    public WorkflowTaskStatus WorkflowTaskStatus { get; init; }
    public string? TaskDescription { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
}