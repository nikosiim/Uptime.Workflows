namespace Uptime.Workflows.Core.Models;

public record ModificationPayload
{
    public string? Executor { get; set; }
    public string? ModificationContext { get; set; }
}