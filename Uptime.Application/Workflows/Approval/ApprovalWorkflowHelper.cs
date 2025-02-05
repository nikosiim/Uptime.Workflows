using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Shared;
using Uptime.Shared.Enums;

namespace Uptime.Application.Workflows.Approval;

public static class ApprovalWorkflowHelper
{
    public static List<ApprovalTaskContext> GetApprovalTasks(int workflowId, ApprovalWorkflowPayload payload)
    {
        return payload.Executors.Select(executor => new ApprovalTaskContext
        {
            WorkflowId = workflowId,
            AssignedBy = payload.Originator,
            AssignedTo = executor,
            TaskDescription = payload.Description,
            DueDate = payload.DueDate
        }).ToList();
    }

    public static string? TryGetComment(this IAlterTaskPayload payload)
    {
        return payload.Storage.TryGetValue(GlobalConstants.TaskStorageKeys.Comment, out object? value) ? value.ToString() : null;
    }

    public static string? TryGetExecutor(this IAlterTaskPayload payload)
    {
        return payload.Storage.TryGetValue(GlobalConstants.TaskStorageKeys.Executor, out object? value) ? value.ToString() : null;
    }

    public static TaskOutcome? TryGetTaskOutcome(this IAlterTaskPayload payload)
    {
        if (payload.Storage.TryGetValue(GlobalConstants.TaskStorageKeys.Outcome, out object? value))
        {
            switch (value)
            {
                case TaskOutcome outcome:
                    return outcome;
                case int intValue when Enum.IsDefined(typeof(TaskOutcome), intValue):
                    return (TaskOutcome)intValue;
                case string strValue when Enum.TryParse(strValue, out TaskOutcome parsedOutcome):
                    return parsedOutcome;
            }
        }

        return null;
    }
}