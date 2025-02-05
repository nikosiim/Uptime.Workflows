using Uptime.Shared.Enums;

namespace Uptime.Application.Models.Approval;

public class AlterTaskPayload
{
    public int WorkflowId { get; set; }
    public int TaskId { get; set; }
    public TaskOutcome Outcome { get; set; }
    public string? Comments { get; set; }
    public string? Executor { get; set; }
}