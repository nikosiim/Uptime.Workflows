using Uptime.Application.Enums;

namespace Uptime.Application.Interfaces;

public interface IReplicator<TItem>
{
    ReplicatorType Type { get; set; }
    IEnumerable<TItem> Items { get; set; }
    Task ExecuteAsync();
    bool IsItemsCompleted { get; }
}