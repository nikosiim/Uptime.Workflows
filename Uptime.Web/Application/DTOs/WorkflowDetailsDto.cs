using Uptime.Shared.Enums;

namespace Uptime.Web.Application.DTOs;

public record WorkflowDetailsDto
{
    public WorkflowStatus Status { get; init; }
    public string? Originator { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int DocumentId { get; init; }
    public string Document { get; init; } = null!;
}