namespace Uptime.Domain.Common;

/// <summary>
/// Serializable wrapper for tuple data in ReplicatorState.
/// </summary>
public class ReplicatorItem
{
    public required object Data { get; set; }
    public Guid TaskGuid { get; set; } = Guid.Empty;
    public bool IsCompleted { get; set; }
}