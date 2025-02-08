﻿using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

internal static class ApprovalWorkflowAuxiliary
{
    public static List<(ApprovalTaskData Data, Guid TaskGuid, bool IsCompleted)> GetApprovalTasks(this IWorkflowPayload payload, WorkflowId workflowId)
    {
        if (!payload.Storage.TryGetValueAsList(TaskStorageKeys.TaskExecutors, out List<string> executors))
            return [];

        var taskDescription = payload.Storage.GetValueAs<string?>(TaskStorageKeys.TaskDescription);
        var dueDate = payload.Storage.GetValueAs<DateTime?>(TaskStorageKeys.TaskDueDate);

        return executors
            .Select(executor => (
                Data: new ApprovalTaskData
                {
                    AssignedBy = payload.Originator,
                    AssignedTo = executor,
                    TaskDescription = taskDescription,
                    DueDate = dueDate
                },
                TaskGuid: Guid.NewGuid(),
                IsCompleted: false
            )).ToList();
    }

}