using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

internal static class ApprovalWorkflowAuxiliary
{
    public static List<ApprovalTaskContext> GetApprovalTasks(this IWorkflowPayload payload, int workflowId)
    {
        if (!payload.Data.TryGetValueAsList(TaskStorageKeys.TaskExecutors, out List<string> executors))
            return [];

        var taskDescription = payload.Data.TryGetValueAs<string>(TaskStorageKeys.TaskDescription);
        var dueDate = payload.Data.TryGetValueAs<DateTime?>(TaskStorageKeys.TaskDueDate);

        return executors.Select(executor => new ApprovalTaskContext
        {
            WorkflowId = workflowId,
            AssignedBy = payload.Originator,
            AssignedTo = executor,
            TaskDescription = taskDescription,
            DueDate = dueDate
        }).ToList();
    }
}