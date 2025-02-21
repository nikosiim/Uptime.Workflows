using Uptime.Domain.Enums;

namespace Uptime.Domain.Common;

/// <summary>
/// Serializable wrapper for tuple data in ReplicatorState.
/// </summary>
public class ReplicatorItem
{
    public required object Data { get; set; }
    public Guid TaskGuid { get; set; } = Guid.Empty;
    public ReplicatorItemStatus Status { get; set; } = ReplicatorItemStatus.NotStarted;
}