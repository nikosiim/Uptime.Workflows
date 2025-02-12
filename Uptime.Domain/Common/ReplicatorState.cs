using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public class ReplicatorState<TItem> where TItem : IReplicatorItem
{
    public ReplicatorType Type { get; set; }
    public List<ReplicatorItem<TItem>> Items { get; set; } = [];
}