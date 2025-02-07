using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Common;

public class WorkflowTaskContext : IWorkflowTask, IReplicatorItem
{
    /* Context Properties */
    public TaskId TaskId { get; set; }
    public WorkflowId WorkflowId { get; set; }
    public bool IsCompleted { get; set; }
    public Dictionary<string, string?> Storage { get; protected set; } = new();
    
    /* Form Data Properties */
    public string AssignedTo { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
}