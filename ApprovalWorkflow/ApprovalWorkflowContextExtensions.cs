using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;

namespace ApprovalWorkflow;

internal static class ApprovalWorkflowStorageKeys
{
    public const string ReplicatorType = "ReplicatorType";
    public const string TaskExecutorsPrincipalIds = "Task.Executors.PrincipalIds";
    public const string TaskSignersPrincipalIds   = "Task.Signers.PrincipalIds";
    public const string TaskDescription = "TaskDescription";
    public const string SignerTask = "SignerTask";
    public const string TaskDueDate = "TaskDueDate";
}

internal static class ApprovalWorkflowContextExtensions
{
    public static ReplicatorType? GetReplicatorType(this IWorkflowContext context)
    {
        if (context.Storage.TryGetValueAsEnum(ApprovalWorkflowStorageKeys.ReplicatorType, out ReplicatorType type))
            return type;

        return null;
    }

    public static List<string> GetApprovalTaskExecutorPrincipalIds(this IWorkflowContext context)
    {
        return context.Storage.TryGetValueAsList(ApprovalWorkflowStorageKeys.TaskExecutorsPrincipalIds, out List<string> ids) ? ids : [];
    }

    public static List<string> GetSigningTaskPrincipalIds(this IWorkflowContext context)
    {
        return context.Storage.TryGetValueAsList(ApprovalWorkflowStorageKeys.TaskSignersPrincipalIds, out List<string> ids) ? ids : [];
    }

    public static string? GetTaskDescription(this IWorkflowContext context)
        => context.Storage.GetValue(ApprovalWorkflowStorageKeys.TaskDescription);

    public static string? GetSignerTaskDescription(this IWorkflowContext context)
        => context.Storage.GetValue(ApprovalWorkflowStorageKeys.SignerTask);

    public static DateTime GetTaskDueDate(this IWorkflowContext context)
        => context.Storage.GetValueAsDateTime(ApprovalWorkflowStorageKeys.TaskDueDate);
}