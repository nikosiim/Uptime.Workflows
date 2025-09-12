namespace Uptime.Shared.Models.Tasks;

public record AlterTaskRequest
{
    public required string CallerSid { get; init; }
    public Dictionary<string, string?> Input { get; init; } = new();
}