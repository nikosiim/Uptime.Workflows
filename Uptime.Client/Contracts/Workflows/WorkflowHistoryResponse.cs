
namespace Uptime.Client.Contracts;

public record WorkflowHistoryResponse
{
    public int Id { get; init; }
    public required string Event { get; init; }
    public string? User { get; init; }
    public string? Comment { get; init; }
    public DateTimeOffset Occurred { get; init; }
    public string? Description { get; init; }
    public int WorkflowId { get; init; }
}