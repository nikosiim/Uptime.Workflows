using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

// TODO: add history entries
// TODO: add overall exception handling and set final workflow state
public class ApprovalWorkflow(IWorkflowService workflowService, ITaskService taskService)
    : WorkflowBase<ApprovalWorkflowContext>(workflowService)
{
    private ITaskService TaskService { get; } = taskService;
    
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

    protected override void OnWorkflowActivated(int workflowId, IWorkflowPayload payload)
    {
        WorkflowContext.ReplicatorState = new ReplicatorState<ApprovalTaskContext>
        {
            Type = ReplicatorType.Sequential,
            Items = payload.GetApprovalTasks(workflowId)
        };
    }

    public async Task<WorkflowStatus> AlterTaskAsync(AlterTaskPayload payload)
    {
        ApprovalTaskContext? taskContext = WorkflowContext.ReplicatorState.Items.FirstOrDefault(t => t.Id == payload.TaskId);
        if (taskContext == null)
        {
            // TODO: log
            return Machine.State;
        }

        var taskActivity = new ApprovalTaskActivity(TaskService, taskContext);
        await taskActivity.OnTaskChanged(payload);

        await RunReplicatorAsync();

        return await CommitWorkflowStateAsync();
    }

    #region Replicator events
    
    private async Task RunReplicatorAsync()
    {
        ReplicatorState<ApprovalTaskContext> approvalReplicator = WorkflowContext.ReplicatorState;

        var replicator = new Replicator<ApprovalTaskContext>
        {
            Type = approvalReplicator.Type,
            Items = approvalReplicator.Items,
            ChildActivityFactory = item => new ApprovalTaskActivity(TaskService, item),
            OnChildInitialized = ReplicatorChildInitialized,
            OnChildCompleted = ReplicatorChildCompleted
        };

        await replicator.ExecuteAsync();

        if (replicator.IsItemsCompleted)
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

    private static void ReplicatorChildInitialized(ApprovalTaskContext input, IWorkflowActivity activity)
    {
        if (activity is ApprovalTaskActivity child)
        {
            // In case activity input data need to be changed

            ApprovalTaskContext currentTask = child.Context;

            DateTime date = DateTime.Now.AddDays(1);
            if (currentTask.DueDate.HasValue)
            {
                date = currentTask.DueDate.Value.AddDays(1);
            }

            child.Context.DueDate = date;
        }
    }

    private void ReplicatorChildCompleted(ApprovalTaskContext item, IWorkflowActivity activity)
    {
        if (activity is ApprovalTaskActivity child)
        {
            ApprovalTaskContext task = child.Context;

            if (task.Storage.GetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome) == TaskOutcome.Rejected)
            {
                WorkflowContext.AnyTaskRejected = true;
                return;
            }

            var delegatedTo = task.Storage.GetValueAs<string?>(TaskStorageKeys.TaskDelegatedTo);
            if (!string.IsNullOrWhiteSpace(delegatedTo))
            {
                var delegatedCtx = new ApprovalTaskContext(task)
                {
                    AssignedTo = delegatedTo
                };

                WorkflowContext.ReplicatorState.Items.Add(delegatedCtx);
            }
        }
    }

    #endregion
}