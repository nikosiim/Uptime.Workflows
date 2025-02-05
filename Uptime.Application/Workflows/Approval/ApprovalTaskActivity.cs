using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Shared.Enums;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalTaskActivity(ITaskService taskService, ApprovalTaskContext context)
    : UserTaskActivity(taskService), IWorkflowActivity
{
    public ApprovalTaskContext Context { get; set; } = context;

    public override async Task ExecuteAsync()
    {
        Context.IsCompleted = false;
        Context.Storage[TaskStorageKeys.Title] = "Kinnitamine";
        Context.Storage[TaskStorageKeys.Outcome] = TaskOutcome.Pending;
        Context.Id = await TaskService.CreateWorkflowTaskAsync(Context);
    }

    public override async Task OnTaskChanged(IAlterTaskPayload payload)
    {
        string? comment = payload.TryGetComment();
        string? executor = payload.TryGetExecutor();
        TaskOutcome? outcome = payload.TryGetTaskOutcome();

        if (outcome is not null)
        {
            switch (outcome)
            {
                case TaskOutcome.Approved:
                    await SetTaskCompleted(comment);
                    break;
                case TaskOutcome.Rejected:
                    await SetTaskRejected(comment);
                    break;
                case TaskOutcome.Delegated:
                    await SetTaskDelegated(comment);
                    break;
            }
        }
    }

    private async Task SetTaskCompleted(string? comment)
    {
        Context.IsCompleted = true;
        Context.Storage[TaskStorageKeys.Outcome] = TaskOutcome.Approved;
        Context.Storage[TaskStorageKeys.Comment] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskRejected(string? comment)
    {
        Context.IsCompleted = true;
        Context.Storage[TaskStorageKeys.Outcome] = TaskOutcome.Rejected;
        Context.Storage[TaskStorageKeys.Comment] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskDelegated(string? comment)
    {
        Context.IsCompleted = true;
        Context.Storage[TaskStorageKeys.Delegated] = "Delegated User";
        Context.Storage[TaskStorageKeys.Outcome] = TaskOutcome.Delegated;
        Context.Storage[TaskStorageKeys.Comment] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }
}