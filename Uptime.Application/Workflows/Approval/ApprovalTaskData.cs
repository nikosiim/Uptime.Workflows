﻿using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalTaskData : UserTaskActivityData
{
    public static ApprovalTaskData Copy(IUserTaskActivityData source)
    {
        return new ApprovalTaskData
        {
            AssignedBy = source.AssignedBy,
            AssignedTo = source.AssignedTo,
            TaskDescription = source.TaskDescription,
            DueDate = source.DueDate
        };
    }
}