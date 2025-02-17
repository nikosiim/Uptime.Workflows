namespace Uptime.Web.Application.DTOs;

public record WorkflowDetailsDto
{
    public string? Outcome { get; init; }
    public string? Originator { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int DocumentId { get; init; }
    public string Document { get; init; } = null!;
    public bool IsActive { get; init; }
}