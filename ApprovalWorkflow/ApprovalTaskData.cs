using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;

namespace ApprovalWorkflow;

public sealed class ApprovalTaskData : UserTaskActivityData
{
    public static ApprovalTaskData Copy(IUserTaskActivityData source, PrincipalId assignedToPrincipalId)
    {
        return new ApprovalTaskData
        {
            AssignedByPrincipalId = source.AssignedByPrincipalId,
            AssignedToPrincipalId = assignedToPrincipalId,
            TaskDescription = source.TaskDescription,
            DueDate = source.DueDate
        };
    }
}