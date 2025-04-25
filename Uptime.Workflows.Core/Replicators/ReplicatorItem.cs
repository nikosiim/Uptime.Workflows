using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core;

public sealed class ReplicatorItem(Guid taskGuid, object data)
{
    public Guid TaskGuid { get; set; } = taskGuid;
    public object Data { get; set; } = data;
    public ReplicatorItemStatus Status { get; set; }
}