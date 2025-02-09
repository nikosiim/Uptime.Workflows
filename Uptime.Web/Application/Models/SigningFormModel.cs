namespace Uptime.Web.Application.Models;

public class SigningFormModel : IWorkflowFormModel
{
    public string TemplateName { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public string? Signer { get; set; }
    public int DueDays { get; set; }
}

public sealed class SigningInitFormModel : SigningFormModel;