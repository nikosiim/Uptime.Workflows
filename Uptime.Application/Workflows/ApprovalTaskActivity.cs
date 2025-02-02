using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Domain.Enums;

namespace Uptime.Application.Workflows;

public class ApprovalTaskActivity(ITaskService taskService, ApprovalTaskContext context)
    : UserTaskActivity(taskService), IWorkflowActivity
{
    public ApprovalTaskContext Context { get; set; } = context;

    public override async Task ExecuteAsync()
    {
        Context.IsCompleted = false;
        Context.Outcome = TaskOutcome.Pending;
        Context.Id = await TaskService.CreateWorkflowTaskAsync(Context);
    }

    public override async Task OnTaskChanged(TaskCompletionPayload payload)
    {
        string? comment = payload.Comments;

        switch (payload.Outcome)
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

    private async Task SetTaskCompleted(string? comment)
    {
        Context.IsCompleted = true;
        Context.Outcome = TaskOutcome.Approved;
        Context.Storage["Comment"] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskRejected(string? comment)
    {
        Context.IsCompleted = true;
        Context.Outcome = TaskOutcome.Rejected;
        Context.Storage["Comment"] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskDelegated(string? comment)
    {
        Context.IsCompleted = true;
        Context.Outcome = TaskOutcome.Delegated;
        Context.Storage["DelegatedTo"] = "Delegated User";
        Context.Storage["Comment"] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }
}