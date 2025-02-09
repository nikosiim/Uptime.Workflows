using Uptime.Application.Enums;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public class ReplicatorState<TItem> where TItem : IReplicatorItem
{
    public ReplicatorType Type { get; set; }
    public List<ReplicatorItem<TItem>> Items { get; set; } = [];
}