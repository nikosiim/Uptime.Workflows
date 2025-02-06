namespace Uptime.Shared.Models.Tasks;

public record AlterTaskRequest
{
    public int WorkflowId { get; init; }
    public Dictionary<string, object> Storage { get; init; } = new();
}