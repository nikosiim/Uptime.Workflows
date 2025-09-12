using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

namespace ApprovalWorkflow;

internal static class ApprovalWorkflowDataExtractor
{
    internal static ReplicatorType GetReplicatorType(this IWorkflowContext workflowContext, string phaseId)
    {
        if (phaseId == ExtendedState.Approval.Value)
        {
            return workflowContext.GetReplicatorType() ?? ReplicatorType.Sequential;
        }
        if (phaseId == ExtendedState.Signing.Value)
        {
            return ReplicatorType.Sequential;
        }
        throw new InvalidOperationException($"Unknown phase: {phaseId}");
    }

    internal static List<ApprovalTaskData> GetApprovalTasks(this IWorkflowContext workflowContext)
    {
        List<string> executorIds = workflowContext.GetApprovalTaskExecutorPrincipalIds();
        string? taskDescription = workflowContext.GetTaskDescription();
        DateTime dueDate = workflowContext.GetTaskDueDate();
        PrincipalId initiatorId = workflowContext.GetInitiatorId();

        return executorIds.Select(text => new ApprovalTaskData
        {
            AssignedByPrincipalId = initiatorId,
            AssignedToPrincipalId = PrincipalId.Parse(text),
            TaskDescription = taskDescription,
            DueDate = dueDate
        }).ToList();
    }

    internal static List<UserTaskActivityData> GetSigningTasks(this IWorkflowContext workflowContext)
    {
        List<string> signerIds = workflowContext.GetSigningTaskPrincipalIds();
        string? taskDescription = workflowContext.GetSignerTaskDescription();
        DateTime dueDate = workflowContext.GetTaskDueDate();
        PrincipalId initiatorId = workflowContext.GetInitiatorId();

        return signerIds.Select(text => new UserTaskActivityData
        {
            AssignedByPrincipalId = initiatorId,
            AssignedToPrincipalId = PrincipalId.Parse(text),
            TaskDescription = taskDescription,
            DueDate = dueDate
        }).ToList();
    }
}