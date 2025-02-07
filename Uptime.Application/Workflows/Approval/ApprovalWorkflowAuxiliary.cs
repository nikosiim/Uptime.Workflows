using Uptime.Application.Interfaces;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

internal static class ApprovalWorkflowAuxiliary
{
    public static List<ApprovalTaskContext> GetApprovalTasks(this IWorkflowPayload payload, int workflowId)
    {
        if (payload.Storage.TryGetValueAsList(TaskStorageKeys.TaskExecutors, out List<string> executors))
        {
            var taskDescription = payload.Storage.GetValueAs<string?>(TaskStorageKeys.TaskDescription);
            var dueDate = payload.Storage.GetValueAs<DateTime?>(TaskStorageKeys.TaskDueDate);

            return executors.Select(executor => new ApprovalTaskContext
            {
                WorkflowId = workflowId,
                AssignedBy = payload.Originator,
                AssignedTo = executor,
                TaskDescription = taskDescription,
                DueDate = dueDate
            }).ToList();
        }

        return [];
    }
}