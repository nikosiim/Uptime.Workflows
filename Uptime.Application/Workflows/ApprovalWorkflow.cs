using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Application.Models.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Workflows;

public class ApprovalWorkflow(ITaskService taskService, WorkflowStatus workflowState, ApprovalWorkflowContext workflowContext)
    : WorkflowBase<ApprovalWorkflowContext>(workflowState, workflowContext)
{
    private ITaskService TaskService { get; } = taskService;
    public ApprovalWorkflowContext Context { get; } = workflowContext;
    
    protected override void OnWorkflowExecuted()
    {
        // TODO: add any initialization logic here
    }

    protected override void ConfigureStateMachine()
    {
        Machine.Configure(WorkflowStatus.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowStatus.InProgress);

        Machine.Configure(WorkflowStatus.InProgress)
            .OnEntryAsync(RunReplicatorActivities)
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowStatus.Completed)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowStatus.Rejected);

        Machine.Configure(WorkflowStatus.Completed)
            .OnEntry(() => Console.WriteLine("Workflow completed successfully."));

        Machine.Configure(WorkflowStatus.Rejected)
            .OnEntry(() => Console.WriteLine("Workflow was rejected."));
    }

    public async Task<bool> CompleteTaskAsync(TaskCompletionPayload payload)
    {
        ApprovalTaskContext? taskContext = Context.ReplicatorState.Items.FirstOrDefault(t => t.Id == payload.TaskId);
        if (taskContext == null)
            return false;

        var taskActivity = new ApprovalTaskActivity(TaskService, taskContext);
        await taskActivity.OnTaskChanged(payload);

        await RunReplicatorActivities();

        return true;
    }
    
    private async Task RunReplicatorActivities()
    {
        ReplicatorState<ApprovalTaskContext> approvalReplicator = WorkflowContext.ReplicatorState;

        var replicator = new Replicator<ApprovalTaskContext>
        {
            Type = approvalReplicator.Type,
            Items = approvalReplicator.Items,
            ChildActivityFactory = item => new ApprovalTaskActivity(TaskService, item),
            OnChildInitialized = InitializeChildActivity,
            OnChildCompleted = HandleChildCompletion
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

    private static void InitializeChildActivity(ApprovalTaskContext input, IWorkflowActivity activity)
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

    private void HandleChildCompletion(ApprovalTaskContext item, IWorkflowActivity activity)
    {
        if (activity is ApprovalTaskActivity child)
        {
            ApprovalTaskContext ctx = child.Context;

            // Task is rejected -> workflow will be cancelled
            if (ctx.Outcome == TaskOutcome.Rejected)
            {
                Context.AnyTaskRejected = true;
            }

            if (ctx.Storage.TryGetValue("DelegatedTo", out object? delegatedTo) && delegatedTo is string delegatedToUser)
            {
                var delegatedCtx = new ApprovalTaskContext(ctx)
                {
                    AssignedTo = delegatedToUser
                };

                WorkflowContext.ReplicatorState.Items.Add(delegatedCtx);
            }
        }
    }
}