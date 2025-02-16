using Uptime.Domain.Enums;

namespace Uptime.Application.DTOs;

public class WorkflowHistoryDto
{
    public required WorkflowHistoryEventType Event { get; set; }
    public required string User { get; set; }
    public string? Outcome { get; set; }
    public DateTime Occurred { get; set; }
    public string? Description { get; set; }
    public int WorkflowId { get; set; }
}