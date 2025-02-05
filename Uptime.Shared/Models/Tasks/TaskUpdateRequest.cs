namespace Uptime.Shared.Models.Tasks;

public record TaskUpdateRequest
{
    public Dictionary<string, object> Storage { get; init; } = new();
}