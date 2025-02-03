using Uptime.Application.Interfaces;
using Uptime.Application.Models.Common;

namespace Uptime.Application.Common;

public class WorkflowTaskBase : IWorkflowTask, IWorkflowItem
{
    /* Context Properties */
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public bool IsCompleted { get; set; }
    public Dictionary<string, object?> Storage { get; protected set; } = new();
    
    /* Form Data Properties */
    public string AssignedTo { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
}