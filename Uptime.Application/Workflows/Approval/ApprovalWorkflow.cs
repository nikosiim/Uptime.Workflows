using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(IWorkflowService workflowService, ITaskService taskService)
    : ReplicatorWorkflowBase<ApprovalWorkflowContext, ApprovalTaskData>(workflowService, taskService)
{
    protected override void ConfigureStateMachine()
    {
        Machine.Configure(WorkflowStatus.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowStatus.InProgress);

        Machine.Configure(WorkflowStatus.InProgress)
            .OnEntryAsync(RunReplicatorsAsync)
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowStatus.Completed)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowStatus.Rejected);

        Machine.Configure(WorkflowStatus.Completed)
            .OnEntry(() => Console.WriteLine("Workflow completed successfully."));

        Machine.Configure(WorkflowStatus.Rejected)
            .OnEntry(() => Console.WriteLine("Workflow was rejected."));
    }

    protected override void OnWorkflowActivated(WorkflowId workflowId, IWorkflowPayload payload)
    {
        WorkflowContext.ReplicatorState = new ReplicatorState<ApprovalTaskData>
        {
            Type = ReplicatorType.Sequential,
            Items = payload.GetApprovalTasks(workflowId) // Kuidas tagada, et siin väärtustatakse taski guidid?
        };
    }

    protected override List<ReplicatorState<ApprovalTaskData>> GetReplicatorStates()
    {
        return [WorkflowContext.ReplicatorState];
    }

    protected override IWorkflowActivity CreateChildActivity(ApprovalTaskData data)
    {
        Guid taskGuid = WorkflowContext.ReplicatorState.Items.First(t => t.Data == data).TaskGuid;
        var taskContext = new WorkflowTaskContext(WorkflowId, taskGuid);
        return new ApprovalTaskActivity(TaskService, taskContext) { InitiationData = data };
    }

    protected override UserTaskActivity CreateChildActivity(WorkflowTaskContext context)
    {
        return new ApprovalTaskActivity(TaskService, context);
    }

    protected override void UpdateReplicatorState(Guid taskGuid, bool isCompleted)
    {
        foreach (ReplicatorItem<ApprovalTaskData> t in WorkflowContext.ReplicatorState.Items.Where(t => t.TaskGuid == taskGuid))
        {
            t.IsCompleted = isCompleted;
            break;
        }
    }

    protected override void OnReplicatorChildCompleted(ApprovalTaskData data, IWorkflowActivity activity)
    {
        if (activity is not ApprovalTaskActivity child) return;

        WorkflowTaskContext task = child.Context;

        // Check if task was rejected
        if (task.Storage.GetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome) == TaskOutcome.Rejected)
        {
            WorkflowContext.AnyTaskRejected = true;
            return;
        }

        // Check if task was delegated
        var delegatedTo = task.Storage.GetValueAs<string?>(TaskStorageKeys.TaskDelegatedTo);

        if (!string.IsNullOrWhiteSpace(delegatedTo))
        {
            // Copy task data and reassign the task to the delegated person
            ApprovalTaskData newData = ApprovalTaskData.Copy(data);
            newData.AssignedTo = delegatedTo;

            WorkflowContext.ReplicatorState.Items.Add(new ReplicatorItem<ApprovalTaskData>
            {
                Data = newData,
                TaskGuid = Guid.NewGuid(),
                IsCompleted = false
            });
        }
    }

    protected override async Task OnAllTasksCompleted(ReplicatorState<ApprovalTaskData> replicatorState)
    {
        if (WorkflowContext.AnyTaskRejected)
        {
            await FireAsync(WorkflowTrigger.TaskRejected);
        }
        else
        {
            await FireAsync(WorkflowTrigger.AllTasksCompleted);
        }
    }
}