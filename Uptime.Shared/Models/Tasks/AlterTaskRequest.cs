namespace Uptime.Shared.Models.Tasks;

public record AlterTaskRequest
{
    public int WorkflowId { get; init; }
    public Dictionary<string, string?> Input { get; init; } = new();
}