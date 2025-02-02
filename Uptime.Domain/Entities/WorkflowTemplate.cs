using System.ComponentModel.DataAnnotations;

namespace Uptime.Domain.Entities;

public class WorkflowTemplate : BaseEntity
{
    [StringLength(128)] 
    public string TemplateName { get; set; } = null!;
    [StringLength(128)]
    public string WorkflowName { get; set; } = null!;
    [StringLength(128)]
    public string WorkflowBaseId { get; set; } = null!;
    [StringLength(2048)]
    public string? AssociationDataJson { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public int LibraryId { get; set; }
    public virtual Library Library { get; set; } = null!;
    public virtual ICollection<Workflow>? Workflows { get; set; }
}