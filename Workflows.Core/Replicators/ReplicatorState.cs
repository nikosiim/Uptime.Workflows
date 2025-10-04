using Workflows.Core.Enums;

namespace Workflows.Core;

public class ReplicatorState
{
    public ReplicatorType ReplicatorType { get; set; }
    public List<ReplicatorItem> Items { get; set; } = [];
    public bool HasTaskGuid(Guid guid) => Items.Any(i => i.TaskGuid == guid);
    public bool HasActiveItems => Items.Any(item => item.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress);
}