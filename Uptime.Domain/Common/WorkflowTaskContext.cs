using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public class WorkflowTaskContext(WorkflowId workflowId, string? phaseId = null) : IWorkflowTask
{
    public WorkflowId WorkflowId { get; } = workflowId;
    public string? PhaseId { get; set; } = phaseId;
    public Guid TaskGuid { get; set;  }
    public TaskId TaskId { get; set; }
    public string AssignedTo { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public WorkflowTaskStatus TaskStatus { get; set; }
    public Dictionary<string, string?> Storage { get; set; } = new();
}