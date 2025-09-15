using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

// TODO: recheck if the new implementation is correct
public sealed class ReplicatorItem(Guid taskGuid, IWorkflowTaskContext taskContext)
{
    public Guid TaskGuid { get; set; } = taskGuid;
    public IWorkflowTaskContext TaskContext { get; set; } = taskContext;
    public ReplicatorItemStatus Status { get; set; }
}