using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public class WorkflowTask : BaseEntity
{
    public Guid TaskGuid { get; set; }
    [StringLength(2048)]
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public WorkflowTaskStatus InternalStatus { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? EndDate { get; set; }
    [StringLength(4096)]
    public string? StorageJson { get; set; }
    [StringLength(32)]
    public string? PhaseId { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;
    public int AssignedToPrincipalId { get; set; }
    public WorkflowPrincipal AssignedTo { get; set; } = null!;
    public int AssignedByPrincipalId { get; set; }
    public WorkflowPrincipal AssignedBy { get; set; } = null!;
}