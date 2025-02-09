using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

internal static class ApprovalWorkflowAuxiliary
{
    public static List<ApprovalTaskData> GetApprovalTasks(this IWorkflowPayload payload, WorkflowId workflowId)
    {
        if (!payload.Storage.TryGetValueAsList(TaskStorageKeys.TaskExecutors, out List<string> executors))
            return [];

        string? taskDescription = payload.Storage.GetValueAsString(TaskStorageKeys.TaskDescription);
        DateTime dueDate = payload.Storage.GetValueAsDateTime(TaskStorageKeys.TaskDueDate);

        return executors.Select(executor => new ApprovalTaskData
        {
            AssignedBy = payload.Originator,
            AssignedTo = executor,
            TaskDescription = taskDescription,
            DueDate = dueDate
        }).ToList();
    }

    public static List<ApprovalTaskData> GetSigningTasks(this IWorkflowPayload payload, WorkflowId workflowId)
    {
        if (!payload.Storage.TryGetValueAsList(TaskStorageKeys.TaskSigners, out List<string> signers))
            return [];

        string? taskDescription = payload.Storage.GetValueAsString(TaskStorageKeys.SignerTask);
        DateTime dueDate = payload.Storage.GetValueAsDateTime(TaskStorageKeys.TaskDueDate);

        return signers.Select(executor => new ApprovalTaskData
        {
            AssignedBy = payload.Originator,
            AssignedTo = executor,
            TaskDescription = taskDescription,
            DueDate = dueDate
        }).ToList();
    }
}