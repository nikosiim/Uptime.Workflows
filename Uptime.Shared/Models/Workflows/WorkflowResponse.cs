using Uptime.Shared.Enums;

namespace Uptime.Shared.Models.Workflows;
public record WorkflowResponse
{
    public WorkflowStatus Status { get; init; }
    public string? Originator { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int DocumentId { get; init; }
    public string Document { get; init; } = null!;
}