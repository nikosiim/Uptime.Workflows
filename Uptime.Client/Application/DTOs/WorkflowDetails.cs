namespace Uptime.Client.Application.DTOs;

public record WorkflowDetails
{
    public int Id { get; init; }
    public required string Phase { get; init; }
    public string? Outcome { get; init; }
    public string? Originator { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public int DocumentId { get; init; }
    public required string Document { get; init; }
    public bool IsActive { get; init; }
    public required string WorkflowBaseId { get; init; }
}