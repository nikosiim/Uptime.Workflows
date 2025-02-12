using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Interfaces;

public interface IReplicator<TItem>
{
    ReplicatorType Type { get; set; }
    List<ReplicatorItem<TItem>> Items { get; set; }
    Task ExecuteAsync();
    bool IsComplete { get; }
}