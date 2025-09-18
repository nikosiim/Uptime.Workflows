namespace Uptime.Client.Contracts;

public record AlterTaskRequest(string ExecutorSid, WorkflowEventType Action)
{
    public Dictionary<string, string?> Input { get; init; } = new();
}