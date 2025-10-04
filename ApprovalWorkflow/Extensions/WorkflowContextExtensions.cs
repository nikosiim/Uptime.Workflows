using Workflows.Core.Enums;
using Workflows.Core.Extensions;
using Workflows.Core.Interfaces;

namespace ApprovalWorkflow;

public enum TaskPhase
{
    Approver,
    Signer
}

internal static class WorkflowContextExtensions
{
    public static ReplicatorType? GetReplicatorType(this IWorkflowContext context)
    {
        if (context.Storage.TryGetValueAsEnum(StorageKeys.ApprovalReplicatorType, out ReplicatorType type))
            return type;

        return null;
    }

    public static DateTime GetTaskDueDate(this IWorkflowContext context, TaskPhase phase)
    {
        string key = phase switch
        {
            TaskPhase.Approver => StorageKeys.TaskApprovalDueDate,
            TaskPhase.Signer   => StorageKeys.TaskSignerDueDate,
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null)
        };
        return context.Storage.GetValueAsDateTime(key);
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
    
    #endregion

    private static class StorageKeys
    {
        public const string ApprovalReplicatorType   = "Workflow.Approval.Task.Approver.ReplicatorType";
        public const string TaskApprovalDueDate      = "Workflow.Approval.Task.Approver.DueDate";
        public const string TaskApproverDescription  = "Workflow.Approval.Task.Approver.Description";
        public const string TaskApproverSids         = "Workflow.Approval.Task.Approver.Sids";
                                                     
        public const string TaskSignerDueDate        = "Workflow.Approval.Task.Signer.DueDate";
        public const string TaskSignerDescription    = "Workflow.Approval.Task.Signer.Description";
        public const string TaskSignerSids           = "Workflow.Approval.Task.Signer.Sids";
    }
}