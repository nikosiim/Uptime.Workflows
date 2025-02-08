using Uptime.Application.Enums;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public class ReplicatorState<TItem> where TItem : IReplicatorItem
{
    public ReplicatorType Type { get; set; }
    public List<ReplicatorItem<TItem>> Items { get; set; } = [];
}

/// <summary>
/// Serializable wrapper for tuple data in ReplicatorState.
/// </summary>
public class ReplicatorItem<TItem>
{
    public required TItem Data { get; set; }
    public required Guid TaskGuid { get; set; }
    public required bool IsCompleted { get; set; }
}