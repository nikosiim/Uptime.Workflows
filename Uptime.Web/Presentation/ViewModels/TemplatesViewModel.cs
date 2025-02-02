using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Presentation.ViewModels;

public class TemplatesViewModel
{
    public int DocumentId { get; set; }
    public List<WorkflowTemplate> Templates { get; set; } = [];
}
