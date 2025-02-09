using Uptime.Domain.Enums;

namespace Uptime.Application.DTOs;

public record WorkflowDto
{
    public WorkflowPhase Phase { get; init; }
    public string? Originator { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int DocumentId { get; init; }
    public string Document { get; init; } = null!;
    public string? InstanceDataJson { get; init; }
}