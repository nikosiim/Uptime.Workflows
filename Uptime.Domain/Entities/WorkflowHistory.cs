using System.ComponentModel.DataAnnotations;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Entities;

public class WorkflowHistory : BaseEntity
{
    public required WorkflowHistoryEventType Event { get; set; }
    [StringLength(128)]
    public required string User { get; set; }
    [StringLength(64)]
    public string? Outcome { get; set; }
    public DateTime Occurred { get; set; }
    [StringLength(4096)]
    public string? Description { get; set; }
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;
}