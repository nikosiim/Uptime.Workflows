using Workflows.Core.Enums;

namespace Workflows.Core.Interfaces;

public interface IReplicator
{
    ReplicatorType Type { get; set; }
    List<ReplicatorItem> Items { get; set; }
    Task ExecuteAsync(CancellationToken ct);
    bool IsComplete { get; }
}