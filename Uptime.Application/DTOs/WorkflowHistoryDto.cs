using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Application.DTOs;

public record WorkflowHistoryDto
{
    public int Id { get; init; }
    public WorkflowEventType Event { get; init; }
    public string? ExecutedBy { get; init; }
    public string? Comment { get; init; }
    public DateTimeOffset Occurred { get; init; }
    public string? Description { get; init; }
    public int WorkflowId { get; init; }
}