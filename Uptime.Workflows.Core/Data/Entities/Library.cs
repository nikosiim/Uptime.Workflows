using System.ComponentModel.DataAnnotations;

namespace Uptime.Workflows.Core.Data;

public class Library : BaseEntity
{
    [StringLength(128)]
    public required string Name { get; set; }
    public DateTime Created { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual ICollection<Document>? Documents { get; set; }
    public virtual ICollection<WorkflowTemplate>? WorkflowTemplates { get; set; }
}