using System.ComponentModel.DataAnnotations;

namespace Uptime.Domain.Entities;

public class WorkflowHistory : BaseEntity
{
    [StringLength(64)]
    public required string Action { get; set; }
    [StringLength(128)]
    public required string Actor { get; set; }
    public DateTime CreatedDate { get; set; }
    [StringLength(512)]
    public string? Comments { get; set; }
    public int WorkflowId { get; set; }
    public virtual Workflow? Workflow { get; set; }
}