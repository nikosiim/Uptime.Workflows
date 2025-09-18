namespace Uptime.Shared.Models.Tasks;

public record AlterTaskRequest(string ExecutorSid)
{
    public Dictionary<string, string?> Input { get; init; } = new();
}