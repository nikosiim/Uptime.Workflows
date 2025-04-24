namespace Uptime.Workflows.Core.Common;

public record ModificationPayload
{
    public string? Executor { get; set; }
    public string? ModificationContext { get; set; }
}