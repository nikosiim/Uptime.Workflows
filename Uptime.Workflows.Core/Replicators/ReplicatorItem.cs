using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

// TODO: recheck if the new implementation is correct
public sealed class ReplicatorItem(Guid taskGuid, IWorkflowActivityContext activityContext)
{
    public Guid TaskGuid { get; set; } = taskGuid;
    public IWorkflowActivityContext ActivityContext { get; set; } = activityContext;
    public ReplicatorItemStatus Status { get; set; }
}