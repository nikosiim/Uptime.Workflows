using Uptime.Client.Application.DTOs;

namespace Uptime.Client.Presentation.ViewModels;

public class WorkflowsViewModel
{
    public List<DocumentWorkflowDto> ActiveInstances { get; set; } = [];
    public List<DocumentWorkflowDto> CompletedInstances { get; set; } = [];
}