using Uptime.Application.Models.Common;

namespace Uptime.Application.Models.Approval;

public sealed class ApprovalWorkflowPayload : WorkflowPayload
{
    public required List<string> Executors { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}