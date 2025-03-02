using Uptime.Domain.Enums;

namespace Uptime.Domain.Common;

public class ReplicatorState
{
    public ReplicatorType ReplicatorType { get; set; }
    public List<ReplicatorItem> Items { get; set; } = [];
    public bool HasTaskGuid(Guid guid) => Items.Any(i => i.TaskGuid == guid);
}