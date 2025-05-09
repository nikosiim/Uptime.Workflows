﻿using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Models;
using static ApprovalWorkflow.Constants;

namespace ApprovalWorkflow;

internal static class ApprovalWorkflowDataExtractor
{
    internal static ReplicatorType GetReplicatorType(this IWorkflowPayload payload, string phaseId)
    {
        var replicatorType = ReplicatorType.Sequential;
        if (payload.Storage.TryGetValueAsEnum(WorkflowStorageKeys.ReplicatorType, out ReplicatorType type))
        {
            replicatorType = type;
        }

        if (phaseId == ExtendedState.Approval.Value)
        {
            return replicatorType;
        }

        if (phaseId == ExtendedState.Signing.Value)
        {
            return ReplicatorType.Sequential;
        }

        throw new InvalidOperationException($"Unknown phase: {phaseId}");
    }

    internal static List<ApprovalTaskData> GetApprovalTasks(this IWorkflowPayload payload, WorkflowId workflowId)
    {
        if (!payload.Storage.TryGetValueAsList(TaskStorageKeys.TaskExecutors, out List<string> executors))
            return [];

        string? taskDescription = payload.Storage.GetValue(TaskStorageKeys.TaskDescription);
        DateTime dueDate = payload.Storage.GetValueAsDateTime(TaskStorageKeys.TaskDueDate);

        return executors.Select(executor => new ApprovalTaskData
        {
            AssignedBy = payload.Originator,
            AssignedTo = executor,
            TaskDescription = taskDescription,
            DueDate = dueDate
        }).ToList();
    }

    internal static List<UserTaskActivityData> GetSigningTasks(this IWorkflowPayload payload, WorkflowId workflowId)
    {
        if (!payload.Storage.TryGetValueAsList(TaskStorageKeys.TaskSigners, out List<string> signers)) 
            return [];
        
        string? taskDescription = payload.Storage.GetValue(TaskStorageKeys.SignerTask);
        DateTime dueDate = payload.Storage.GetValueAsDateTime(TaskStorageKeys.TaskDueDate);

        return signers.Select(signer => new UserTaskActivityData
        {
            AssignedBy = payload.Originator,
            AssignedTo = signer,
            TaskDescription = taskDescription,
            DueDate = dueDate
        }).ToList();
    }
}