﻿using Microsoft.Extensions.Logging;
using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(
    IWorkflowStateRepository<ApprovalWorkflowContext> stateRepository,
    IWorkflowPersistenceService workflowService, 
    ITaskService taskService, 
    IReplicatorPhaseBuilder<ApprovalTaskData> replicatorPhaseBuilder,
    IWorkflowActivityFactory<ApprovalTaskData> activityFactory, 
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorWorkflowBase<ApprovalWorkflowContext, ApprovalTaskData>(stateRepository, workflowService, taskService, activityFactory, replicatorPhaseBuilder, logger)
{
    public static class Phases
    {
        public const string ApprovalPhase = "ApprovalPhase";
        public const string SigningPhase = "SigningPhase";
    }
    
    public static Dictionary<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<ApprovalTaskData>>> PhaseConfiguration
        => new()
        {
            { Phases.ApprovalPhase, (payload, workflowId) => payload.GetApprovalTasks(workflowId) },
            { Phases.SigningPhase, (payload, workflowId) => payload.GetSigningTasks(workflowId) }
        };

    protected override void ConfigureStateMachine()
    {
        Machine.Configure(WorkflowPhase.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowPhase.Approval);

        Machine.Configure(WorkflowPhase.Approval)
            .OnEntryAsync(() => RunReplicatorAsync(Phases.ApprovalPhase))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Signing)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowPhase.Rejected);
        Machine.Configure(WorkflowPhase.Signing)
            .OnEntryAsync(() => RunReplicatorAsync(Phases.SigningPhase))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed);

        Machine.Configure(WorkflowPhase.Completed)
            .OnEntry(() => Console.WriteLine("Workflow completed successfully."));

        Machine.Configure(WorkflowPhase.Rejected)
            .OnEntry(() => Console.WriteLine("Workflow was rejected."));
    }

    protected override void OnWorkflowActivated(IWorkflowPayload payload)
    {
        base.OnWorkflowActivated(payload);
    }
}