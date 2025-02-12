using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public class WorkflowTaskContext(WorkflowId workflowId) : IWorkflowTask
{
    public WorkflowId WorkflowId { get; } = workflowId;
    public Guid TaskGuid { get; set;  }
    public TaskId TaskId { get; set; }
    public string AssignedTo { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public Dictionary<string, string?> Storage { get; set; } = new();
}