namespace Uptime.Shared.Models.Tasks;

public record AlterTaskRequest
{
    public int WorkflowId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}