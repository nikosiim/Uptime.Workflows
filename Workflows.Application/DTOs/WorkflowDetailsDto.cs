namespace Workflows.Application.DTOs;

public record WorkflowDetailsDto
{
    public bool IsActive { get; init; }
    public string? Outcome { get; init; }
    public required string Phase { get; init; }
    public string? Originator { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public int DocumentId { get; init; }
    public required string WorkflowBaseId { get; init; }
}