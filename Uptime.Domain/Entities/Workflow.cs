using System.ComponentModel.DataAnnotations;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Entities;

public class Workflow : BaseEntity
{
    public WorkflowPhase Phase { get; set; }
    [StringLength(128)]
    public string? Originator { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [StringLength(4096)]
    public string? StorageJson { get; set; }
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    public int WorkflowTemplateId { get; set; }
    public virtual WorkflowTemplate WorkflowTemplate { get; set; } = null!;
    public virtual ICollection<WorkflowTask>? WorkflowTasks { get; set; }
    public virtual ICollection<WorkflowHistory>? WorkflowHistories { get; set; }
}