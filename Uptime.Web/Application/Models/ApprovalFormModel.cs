namespace Uptime.Web.Application.Models;

public class ApprovalFormModel : IWorkflowFormModel
{
    public string TemplateName { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
}

public sealed class ApprovalInitFormModel : ApprovalFormModel;