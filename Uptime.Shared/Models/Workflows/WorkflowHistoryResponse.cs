﻿using Uptime.Shared.Enums;

namespace Uptime.Shared.Models.Workflows;

public record WorkflowHistoryResponse
{
    public int Id { get; init; }
    public WorkflowEventType Event { get; init; }
    public string? User { get; init; }
    public string? Comment { get; init; }
    public DateTime Occurred { get; init; }
    public string? Description { get; init; }
    public int WorkflowId { get; init; }
}