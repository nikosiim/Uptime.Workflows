using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Application.DTOs;

public class WorkflowHistoryDto
{
    public int Id { get; set; }
    public WorkflowEventType Event { get; set; }
    public string? PerformedBy { get; set; }
    public string? Comment { get; set; }
    public DateTime Occurred { get; set; }
    public string? Description { get; set; }
    public int WorkflowId { get; set; }
}