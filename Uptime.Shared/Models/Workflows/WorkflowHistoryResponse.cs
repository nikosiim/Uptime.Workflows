using Uptime.Shared.Enums;

namespace Uptime.Shared.Models.Workflows;

public class WorkflowHistoryResponse
{
    public int Id { get; set; }
    public WorkflowHistoryEventType Event { get; set; }
    public string? User { get; set; }
    public string? Outcome { get; set; }
    public DateTime Occurred { get; set; }
    public string? Description { get; set; }
    public int WorkflowId { get; set; }
}