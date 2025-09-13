using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;

namespace ApprovalWorkflow;

internal static class ApprovalWorkflowContextExtensions
{
    public static ReplicatorType? GetReplicatorType(this IWorkflowContext context)
    {
        if (context.Storage.TryGetValueAsEnum(StorageKeys.ReplicatorType, out ReplicatorType type))
            return type;

        return null;
    }
    
    public static DateTime GetTaskDueDate(this IWorkflowContext context)
    {
        return context.Storage.GetValueAsDateTime(StorageKeys.TaskDueDate);
    }

    #region ApproverTask

    public static string? GetTaskApproverDescription(this IWorkflowContext context)
    {
        return context.Storage.GetValue(StorageKeys.TaskApproverDescription);
    }

    public static List<string> GetTaskApproverSids(this IWorkflowContext context)
    {
        return context.Storage.TryGetValueAsList(StorageKeys.TaskApproverSids, out List<string> sids) ? sids : [];
    }
    
    public static List<string> GetTaskApproverPrincipalIds(this IWorkflowContext context)
    {
        return context.Storage.TryGetValueAsList(StorageKeys.TaskApproverPrincipalIds, out List<string> ids) ? ids : [];
    }

    public static void SetTaskApproverPrincipalIds(this IWorkflowContext context, IEnumerable<string> principalIds)
    {
        context.Storage.SetValue(StorageKeys.TaskApproverPrincipalIds, string.Join(DictionaryExtensions.ListSeparator, principalIds));
    }
    
    #endregion

    #region SignerTask

    public static string? GetTaskSignerDescription(this IWorkflowContext context)
    {
        return context.Storage.GetValue(StorageKeys.TaskSignerDescription);
    }

    public static List<string> GetTaskSignerSids(this IWorkflowContext context)
    {
        return context.Storage.TryGetValueAsList(StorageKeys.TaskSignerSids, out List<string> sids) ? sids : [];
    }
    
    public static List<string> GetTaskSignerPrincipalIds(this IWorkflowContext context)
    {
        return context.Storage.TryGetValueAsList(StorageKeys.TaskSignerPrincipalIds, out List<string> ids) ? ids : [];
    }

    public static void SetTaskSignersPrincipalIds(this IWorkflowContext context, IEnumerable<string> principalIds)
    {
        context.Storage.SetValue(StorageKeys.TaskSignerPrincipalIds, string.Join(DictionaryExtensions.ListSeparator, principalIds));
    }
    
    #endregion

    private static class StorageKeys
    {
        public const string ReplicatorType = "Workflow.ReplicatorType";
        public const string TaskDueDate = "Task.DueDate";

        public const string TaskApproverDescription = "Task.Approver.Description";
        public const string TaskApproverSids = "Task.Approver.Sids";
        public const string TaskApproverPrincipalIds = "Task.Approver.PrincipalIds";

        public const string TaskSignerSids = "Task.Signer.Sids";
        public const string TaskSignerDescription = "Task.Signer.Description";
        public const string TaskSignerPrincipalIds = "Task.Signer.PrincipalIds";
    }
}