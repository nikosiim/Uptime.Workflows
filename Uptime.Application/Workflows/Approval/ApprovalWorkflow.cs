using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

// TODO: add history entries
// TODO: add overall exception handling and set final workflow state
public class ApprovalWorkflow(IWorkflowService workflowService, ITaskService taskService)
    : WorkflowBase<ApprovalWorkflowContext>(workflowService)
{
    protected override void ConfigureStateMachine()
    {
        Machine.Configure(WorkflowStatus.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowStatus.InProgress);

        Machine.Configure(WorkflowStatus.InProgress)
            .OnEntryAsync(RunReplicatorAsync)
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
            Items = payload.GetApprovalTasks(workflowId)
        };
    }

    public async Task<WorkflowStatus> AlterTaskAsync(AlterTaskPayload payload)
    {
        WorkflowTaskContext? context = await taskService.GetWorkflowTaskContextAsync(payload.TaskId);
        if (context == null)
            return Machine.State;

        var taskActivity = new ApprovalTaskActivity(taskService, context);
        await taskActivity.OnTaskChanged(payload);

        for (var i = 0; i < WorkflowContext.ReplicatorState.Items.Count; i++)
        {
            (ApprovalTaskData data, Guid taskGuid, bool isCompleted) = WorkflowContext.ReplicatorState.Items[i];

            if (taskGuid == context.TaskGuid)
            {
                WorkflowContext.ReplicatorState.Items[i] = (data, taskGuid, taskActivity.IsCompleted);
                break;
            }
        }

        await RunReplicatorAsync();

        return await CommitWorkflowStateAsync();
    }

    #region Replicator events
    
    private async Task RunReplicatorAsync()
    {
        ReplicatorState<ApprovalTaskData> approvalReplicator = WorkflowContext.ReplicatorState;
        
        var replicator = new Replicator<ApprovalTaskData>
        {
            Type = approvalReplicator.Type,
            Items = approvalReplicator.Items,
            ChildActivityFactory = data =>
            {
                Guid taskGuid = approvalReplicator.Items.First(t => t.Data == data).TaskGuid;
                var taskContext = new WorkflowTaskContext(WorkflowId, taskGuid);

                return new ApprovalTaskActivity(taskService, taskContext) { InitiationData = data };
            },
            OnChildInitialized = ReplicatorChildInitialized,
            OnChildCompleted = ReplicatorChildCompleted
        };

        await replicator.ExecuteAsync();
        
        if (replicator.IsComplete)
        {
            if (WorkflowContext.AnyTaskRejected)
            {
                await Machine.FireAsync(WorkflowTrigger.TaskRejected);
            }
            else
            {
                await Machine.FireAsync(WorkflowTrigger.AllTasksCompleted);
            }
        }
    }

    private static void ReplicatorChildInitialized(ApprovalTaskData data, IWorkflowActivity activity)
    {
        // In case activity input data need to be changed
        if (activity is ApprovalTaskActivity child)
        {
            WorkflowTaskContext currentTask = child.Context;

            // Just to create an example
            DateTime date = DateTime.Now.AddDays(1);
            if (currentTask.DueDate.HasValue)
            {
                date = currentTask.DueDate.Value.AddDays(1);
            }

            child.Context.DueDate = date;
        }
    }

    private void ReplicatorChildCompleted(ApprovalTaskData data, IWorkflowActivity activity)
    {
        if (activity is ApprovalTaskActivity child)
        {
            WorkflowTaskContext task = child.Context;

            // Check if the task was rejected
            if (task.Storage.GetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome) == TaskOutcome.Rejected)
            {
                WorkflowContext.AnyTaskRejected = true;
                return;
            }

            // Check if the task was delegated
            var delegatedTo = task.Storage.GetValueAs<string?>(TaskStorageKeys.TaskDelegatedTo);
            if (!string.IsNullOrWhiteSpace(delegatedTo))
            {
                ApprovalTaskData newData = ApprovalTaskData.Copy(data);
                newData.AssignedTo = delegatedTo;

                WorkflowContext.ReplicatorState.Items.Add((newData, Guid.NewGuid(), false));
            }
        }
    }

    #endregion
}