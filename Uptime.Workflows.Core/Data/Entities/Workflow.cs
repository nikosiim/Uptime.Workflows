using System.ComponentModel.DataAnnotations;

namespace Uptime.Workflows.Core.Data;

public sealed class Workflow : BaseEntity
{
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    [StringLength(64)]
    public string? Outcome { get; set; }
    [StringLength(32)]
    public string Phase { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [StringLength(4096)]
    public string? StorageJson { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    // Navigation properties
    public int InitiatedByPrincipalId { get; set; }
    public WorkflowPrincipal InitiatedByPrincipal { get; set; } = null!;
    public int WorkflowTemplateId { get; set; }
    public WorkflowTemplate WorkflowTemplate { get; set; } = null!;
    public ICollection<WorkflowTask>? WorkflowTasks { get; set; }
    public ICollection<WorkflowHistory>? WorkflowHistories { get; set; }
}