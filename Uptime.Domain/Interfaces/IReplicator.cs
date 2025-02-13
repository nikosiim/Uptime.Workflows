using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Interfaces;

public interface IReplicator
{
    ReplicatorType Type { get; set; }
    List<ReplicatorItem> Items { get; set; }
    Task ExecuteAsync(CancellationToken cancellationToken);
    bool IsComplete { get; }
}