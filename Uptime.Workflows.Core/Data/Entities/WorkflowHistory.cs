﻿using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public class WorkflowHistory : BaseEntity
{
    public required WorkflowEventType Event { get; set; }
    [StringLength(128)]
    public string? User { get; set; }
    public DateTime Occurred { get; set; }
    [StringLength(2048)]
    public string? Description { get; set; }

    [StringLength(2048)]
    public string? Comment { get; set; }
    public bool IsDeleted { get; set; }
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;
}