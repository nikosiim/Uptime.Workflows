namespace Uptime.Client.Application.DTOs;

public record WorkflowDetails
{
    public int Id { get; init; }
    public required string Phase { get; init; }
    public string? Outcome { get; init; }
    public string? Originator { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int DocumentId { get; init; }
    public required string Document { get; init; }
    public bool IsActive { get; init; }
    public required string WorkflowBaseId { get; init; }
}