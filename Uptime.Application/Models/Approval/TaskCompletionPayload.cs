using Uptime.Application.Enums;

namespace Uptime.Application.Models.Approval;

public class TaskCompletionPayload
{
    public int WorkflowId { get; set; }
    public int TaskId { get; set; }
    public TaskOutcome Outcome { get; set; }
    public string? Comments { get; set; }
    public string? PerformedBy { get; set; }
}