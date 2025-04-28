using Uptime.Workflows.Core;

namespace ApprovalWorkflow;

public sealed class ApprovalTaskData : UserTaskActivityData
{
    public static ApprovalTaskData Copy(IUserTaskActivityData source, string assignedTo)
    {
        return new ApprovalTaskData
        {
            AssignedBy = source.AssignedBy,
            AssignedTo = assignedTo,
            TaskDescription = source.TaskDescription,
            DueDate = source.DueDate
        };
    }
}