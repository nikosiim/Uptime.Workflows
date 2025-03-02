namespace Uptime.Shared.Models.Tasks;

public record AlterTaskRequest
{
    public Dictionary<string, string?> Input { get; init; } = new();
}