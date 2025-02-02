using System.ComponentModel.DataAnnotations;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Entities;

public class Workflow : BaseEntity
{
    public WorkflowStatus Status { get; set; }
    [StringLength(4096)]
    public string? InstanceDataJson { get; set; }
    [StringLength(128)]
    public string? Originator { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DocumentId { get; set; }
    public virtual Document? Document { get; set; }
    public int WorkflowTemplateId { get; set; }
    public virtual WorkflowTemplate? WorkflowTemplate { get; set; }
    public virtual ICollection<WorkflowTask>? WorkflowTasks { get; set; }
}