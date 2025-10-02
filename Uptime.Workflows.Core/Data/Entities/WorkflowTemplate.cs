using System.ComponentModel.DataAnnotations;

namespace Uptime.Workflows.Core.Data;

public sealed class WorkflowTemplate : BaseEntity
{
    public Guid LibraryId { get; set; }
    [StringLength(128)] 
    public string TemplateName { get; set; } = null!;
    [StringLength(128)]
    public string WorkflowName { get; set; } = null!;
    [StringLength(128)]
    public string WorkflowBaseId { get; set; } = null!;
    [StringLength(512)]
    public string SiteUrl { get; set; } = null!;
    [StringLength(2048)]
    public string? AssociationDataJson { get; set; }

    // Navigation properties
    public ICollection<Workflow>? Workflows { get; set; }
}