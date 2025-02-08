using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Common;

public class WorkflowTaskContext(WorkflowId workflowId, Guid taskGuid) : IWorkflowTask
{
    public WorkflowId WorkflowId { get; } = workflowId;
    public Guid TaskGuid { get; } = taskGuid;
    public TaskId TaskId { get; set; }
    public string AssignedTo { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public Dictionary<string, string?> Storage { get; set; } = new();
}