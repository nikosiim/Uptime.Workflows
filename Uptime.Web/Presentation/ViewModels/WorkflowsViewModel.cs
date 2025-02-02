using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Presentation.ViewModels;

public class WorkflowsViewModel
{
    public List<DocumentWorkflow> ActiveInstances { get; set; } = [];
    public List<DocumentWorkflow> CompletedInstances { get; set; } = [];
}