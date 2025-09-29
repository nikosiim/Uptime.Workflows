using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core;

public sealed class ReplicatorItem(WorkflowActivityContext activityContext)
{
    public Guid TaskGuid { get; init; } = activityContext.TaskGuid;
    public WorkflowActivityContext ActivityContext { get; set; } = activityContext;
    public ReplicatorItemStatus Status { get; set; }
}