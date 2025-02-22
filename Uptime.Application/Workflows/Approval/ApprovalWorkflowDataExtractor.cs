﻿using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

internal static class ApprovalWorkflowDataExtractor
{
    public static ReplicatorType GetReplicatorType(this IWorkflowPayload payload, string phaseName)
    {
        var replicatorType = ReplicatorType.Sequential;
        if (payload.Storage.TryGetValueAsEnum(WorkflowStorageKeys.ReplicatorType, out ReplicatorType type))
        {
            replicatorType = type;
        }

        return phaseName switch
        {
            ApprovalWorkflow.ReplicatorPhases.ApprovalPhase => replicatorType,
            ApprovalWorkflow.ReplicatorPhases.SigningPhase => ReplicatorType.Sequential,
            _ => throw new InvalidOperationException($"Unknown phase: {phaseName}")
        };
    }

    public static List<ApprovalTaskData> GetApprovalTasks(this IWorkflowPayload payload, WorkflowId workflowId)
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

    public static List<SigningTaskData> GetSigningTasks(this IWorkflowPayload payload, WorkflowId workflowId)
    {
        if (!payload.Storage.TryGetValueAsList(TaskStorageKeys.TaskSigners, out List<string> signers)) 
            return [];
        
        string? taskDescription = payload.Storage.GetValue(TaskStorageKeys.SignerTask);
        DateTime dueDate = payload.Storage.GetValueAsDateTime(TaskStorageKeys.TaskDueDate);

        return signers.Select(signer => new SigningTaskData
        {
            AssignedBy = payload.Originator,
            AssignedTo = signer,
            TaskDescription = taskDescription,
            DueDate = dueDate
        }).ToList();
    }
}