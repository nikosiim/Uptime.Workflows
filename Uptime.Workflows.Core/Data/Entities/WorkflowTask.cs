using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public sealed class WorkflowTask : BaseEntity
{
    public Guid TaskGuid { get; private set; }
    public WorkflowTaskStatus InternalStatus { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    [StringLength(2048)]
    public string? Description { get; set; }
    [StringLength(4096)]
    public string? StorageJson { get; set; }
    [StringLength(32)]
    public string? PhaseId { get; set; }

    // Navigation properties
    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    public int AssignedToId { get; set; }
    public WorkflowPrincipal AssignedTo { get; set; } = null!;
    public int AssignedById { get; set; }
    public WorkflowPrincipal AssignedBy { get; set; } = null!;

    public void SetTaskGuid(Guid value)
    {
        if (TaskGuid != Guid.Empty && TaskGuid != value) 
            throw new InvalidOperationException("TaskGuid is immutable once set.");
        TaskGuid = value;
    }
}