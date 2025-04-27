using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core;

public interface IReplicator
{
    ReplicatorType Type { get; set; }
    List<ReplicatorItem> Items { get; set; }
    Task ExecuteAsync(CancellationToken cancellationToken);
    bool IsComplete { get; }
}