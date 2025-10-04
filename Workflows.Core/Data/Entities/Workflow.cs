using System.ComponentModel.DataAnnotations;

namespace Workflows.Core.Data;

public sealed class Workflow : BaseEntity
{
    public int DocumentId { get; set; }
    public bool IsActive { get; set; }
    [StringLength(64)]
    public string? Outcome { get; set; }
    [StringLength(32)]
    public string Phase { get; set; } = null!;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    [StringLength(4096)]
    public string? StorageJson { get; set; }

    // Navigation properties
    public int InitiatedById { get; set; }
    public WorkflowPrincipal InitiatedBy { get; set; } = null!;
    public int WorkflowTemplateId { get; set; }
    public WorkflowTemplate WorkflowTemplate { get; set; } = null!;
    public ICollection<WorkflowTask>? WorkflowTasks { get; set; }
    public ICollection<WorkflowHistory>? WorkflowHistories { get; set; }
}