using System.ComponentModel.DataAnnotations;

namespace Uptime.Workflows.Core.Data;

public sealed class Library : BaseEntity
{
    [StringLength(128)]
    public required string Name { get; set; }

    // Navigation properties
    public ICollection<Document>? Documents { get; set; }
    public ICollection<WorkflowTemplate>? WorkflowTemplates { get; set; }
}