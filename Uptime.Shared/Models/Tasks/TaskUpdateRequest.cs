namespace Uptime.Shared.Models.Tasks;

public record TaskUpdateRequest
{
    public int WorkflowId { get; init; }
    public Dictionary<string, object> Storage { get; init; } = new();
}