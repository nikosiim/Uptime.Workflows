using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public sealed class WorkflowHistory : IEntity
{
    public int Id { get; init; }
    public required WorkflowEventType Event { get; set; }
    public DateTime Occurred { get; set; }
    [StringLength(2048)]
    public string? Description { get; set; }
    [StringLength(2048)]
    public string? Comment { get; set; }

    // Navigation properties
    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    public int PerformedById { get; set; }
    public WorkflowPrincipal PerformedBy { get; set; } = null!;
}