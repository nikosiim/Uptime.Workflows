using Uptime.Client.Contracts;

namespace Uptime.Client.Application.Common;

public class ApprovalFormModel : IWorkflowFormModel
{
    public ReplicatorType ReplicatorType { get; set; }
    public string TemplateName { get; set; } = null!;
    public string Originator { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public IEnumerable<string> AssignedTo { get; set; } = [];
    public string? Signer { get; set; }
}