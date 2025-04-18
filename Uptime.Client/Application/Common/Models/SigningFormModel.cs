namespace Uptime.Client.Application.Common;

public class SigningFormModel : IWorkflowFormModel
{
    public string TemplateName { get; set; } = null!;
    public string Originator { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public string? Signer { get; set; }
    public int DueDays { get; set; }
}