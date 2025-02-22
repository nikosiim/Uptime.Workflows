using Uptime.Shared.Enums;

namespace Uptime.Web.Application.Models;

public class ApprovalFormModel : IWorkflowFormModel
{
    public ReplicatorType ReplicatorType { get; set; }
    public string TemplateName { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public IEnumerable<string> AssignedTo { get; set; } = [];
    public string? Signer { get; set; }
}

public sealed class ApprovalInitFormModel : ApprovalFormModel;