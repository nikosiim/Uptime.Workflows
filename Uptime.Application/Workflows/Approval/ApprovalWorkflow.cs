using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(IWorkflowService workflowService, ITaskService taskService, IWorkflowActivityFactory<ApprovalTaskData> activityFactory)
    : ReplicatorWorkflowBase<ApprovalWorkflowContext, ApprovalTaskData>(workflowService, taskService, activityFactory)
{
    public static class Phases
    {
        public const string ApprovalPhase = "ApprovalPhase";
        public const string SigningPhase = "SigningPhase";
    }

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

    protected override List<ReplicatorPhase<ApprovalTaskData>> GetReplicatorPhases(IWorkflowPayload payload, WorkflowId workflowId)
    {
        return
        [
            new ReplicatorPhase<ApprovalTaskData>
            {
                PhaseName = Phases.ApprovalPhase,
                TaskData = payload.GetApprovalTasks(workflowId)
            },

            new ReplicatorPhase<ApprovalTaskData>
            {
                PhaseName = Phases.SigningPhase,
                TaskData = payload.GetSigningTasks(workflowId)
            }
        ];
    }
}