using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(IWorkflowService workflowService, ITaskService taskService, IWorkflowActivityFactory<ApprovalTaskData> activityFactory)
    : ReplicatorWorkflowBase<ApprovalWorkflowContext, ApprovalTaskData>(workflowService, taskService, activityFactory)
{
    protected override void ConfigureStateMachine()
    {
        Machine.Configure(WorkflowStatus.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowStatus.ApprovalInProgress);

        Machine.Configure(WorkflowStatus.ApprovalInProgress)
            .OnEntryAsync(() => RunReplicatorAsync(WorkflowStatus.ApprovalInProgress.ToString()))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowStatus.SigningInProgress)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowStatus.Rejected);

        Machine.Configure(WorkflowStatus.SigningInProgress)
            .OnEntryAsync(() => RunReplicatorAsync(WorkflowStatus.SigningInProgress.ToString()))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowStatus.Completed);

        Machine.Configure(WorkflowStatus.Completed)
            .OnEntry(() => Console.WriteLine("Workflow completed successfully."));

        Machine.Configure(WorkflowStatus.Rejected)
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
                PhaseName = "ApprovalInProgress",
                TaskData = payload.GetApprovalTasks(workflowId)
            },

            new ReplicatorPhase<ApprovalTaskData>
            {
                PhaseName = "SigningInProgress",
                TaskData = payload.GetSigningTasks(workflowId)
            }
        ];
    }
}