using System.ComponentModel.DataAnnotations;

namespace Uptime.Workflows.Core.Data;

public sealed class Document : BaseEntity
{
    [StringLength(128)]
    public string Title { get; set; } = null!;
    [StringLength(512)]
    public string? Description { get; set; }

    // Navigation properties
    public int LibraryId { get; set; }
    public Library Library { get; set; } = null!;
    public ICollection<Workflow>? Workflows { get; set; }
}