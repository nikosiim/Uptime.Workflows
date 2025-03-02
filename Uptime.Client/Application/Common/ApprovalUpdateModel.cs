namespace Uptime.Client.Application.Common;

public sealed class ApprovalUpdateModel
{
    public int WorkflowId { get; set; }
    public IEnumerable<string> AssignedTo { get; set; } = [];
}