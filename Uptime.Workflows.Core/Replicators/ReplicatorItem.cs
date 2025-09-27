using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public sealed class ReplicatorItem(Guid taskGuid, IWorkflowActivityContext activityContext)
{
    public Guid TaskGuid { get; set; } = taskGuid;
    public IWorkflowActivityContext ActivityContext { get; set; } = activityContext;
    public ReplicatorItemStatus Status { get; set; }
}