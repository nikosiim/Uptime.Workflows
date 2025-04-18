using Uptime.Domain.Enums;

namespace Uptime.Application.DTOs;

public record DocumentWorkflowTaskDto
{
    public int TaskId { get; init; }
    public int WorkflowId { get; init; }
    public string? AssignedTo { get; init; }
    public string Status { get; init; } = null!;
    public WorkflowTaskStatus WorkflowTaskStatus { get; init; }
    public string? TaskDescription { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? EndDate { get; init; }
}