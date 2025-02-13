using Uptime.Domain.Enums;

namespace Uptime.Domain.Common;

public class ReplicatorState
{
    public ReplicatorType Type { get; set; }
    public List<ReplicatorItem> Items { get; set; } = [];
}