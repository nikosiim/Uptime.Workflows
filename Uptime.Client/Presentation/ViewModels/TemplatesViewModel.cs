using Uptime.Client.Application.DTOs;

namespace Uptime.Client.Presentation.ViewModels;

public class TemplatesViewModel
{
    public int DocumentId { get; set; }
    public List<WorkflowTemplateDto> Templates { get; set; } = [];
}
