using System.ComponentModel.DataAnnotations;

namespace Uptime.Domain.Entities;

public class Document : BaseEntity
{
    [StringLength(128)]
    public string Title { get; set; } = null!;
    [StringLength(512)]
    public string? Description { get; set; }
    [StringLength(128)]
    public string? CreatedBy { get; set; }
    public DateTime Created { get; set; }
    public bool IsDeleted { get; set; }
    public int LibraryId { get; set; }
    public virtual Library Library { get; set; } = null!;
    public virtual ICollection<Workflow>? Workflows { get; set; }
}