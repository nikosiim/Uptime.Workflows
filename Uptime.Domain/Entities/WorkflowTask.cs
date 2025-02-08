using System.ComponentModel.DataAnnotations;
using Uptime.Shared.Enums;

namespace Uptime.Domain.Entities;

public class WorkflowTask : BaseEntity
{
    public Guid TaskGuid { get; set; }

    [StringLength(128)]
    public string AssignedTo { get; set; } = null!;
    [StringLength(128)]
    public string AssignedBy { get; set; } = null!;
    [StringLength(2048)]
    public string? Description { get; set; }
    public WorkflowTaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? EndDate { get; set; }
    [StringLength(4096)]
    public string? StorageJson { get; set; }
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;
}