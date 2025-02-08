using Uptime.Application.Enums;

namespace Uptime.Application.Interfaces;

public interface IReplicator<TItem>
{
    ReplicatorType Type { get; set; }
    List<(TItem Data, Guid TaskGuid, bool IsCompleted)> Items { get; set; }
    Task ExecuteAsync();
    bool IsComplete { get; }
}