using System.ComponentModel.DataAnnotations;

namespace Uptime.Workflows.Core.Entities;

public class Library : BaseEntity
{
    [StringLength(128)]
    public required string Name { get; set; }
    public DateTime Created { get; set; }
    public bool IsDeleted { get; set; }
    public virtual ICollection<Document>? Documents { get; set; }
    public virtual ICollection<WorkflowTemplate>? WorkflowTemplates { get; set; }
}