using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Presentation.ViewModels;

public class WorkflowsViewModel
{
    public List<DocumentWorkflowDto> ActiveInstances { get; set; } = [];
    public List<DocumentWorkflowDto> CompletedInstances { get; set; } = [];
}