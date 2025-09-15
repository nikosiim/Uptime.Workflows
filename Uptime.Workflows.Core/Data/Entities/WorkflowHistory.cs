using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public sealed class WorkflowHistory : BaseEntity
{
    public required WorkflowEventType Event { get; set; }
    public DateTime Occurred { get; set; }
    [StringLength(2048)]
    public string? Description { get; set; }
    [StringLength(2048)]
    public string? Comment { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    public int PerformedByPrincipalId { get; set; }
    public WorkflowPrincipal PerformedByPrincipal { get; set; } = null!;
}