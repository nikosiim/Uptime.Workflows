namespace Uptime.Application.DTOs;

public record WorkflowDetailsDto
{
    public bool IsActive { get; init; }
    public string? Outcome { get; init; }
    public string Phase { get; init; } = null!;
    public string? Originator { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int DocumentId { get; init; }
    public string Document { get; init; } = null!;
    public string? InstanceDataJson { get; init; }
}