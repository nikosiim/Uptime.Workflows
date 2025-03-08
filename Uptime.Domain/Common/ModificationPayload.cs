namespace Uptime.Domain.Common;

public record ModificationPayload
{
    public string? Executor { get; set; }
    public string? ModificationContext { get; set; }
}