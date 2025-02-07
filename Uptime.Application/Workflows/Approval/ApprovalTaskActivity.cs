using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalTaskActivity(ITaskService taskService, ApprovalTaskContext context)
    : UserTaskActivity(taskService), IWorkflowActivity
{
    public ApprovalTaskContext Context { get; set; } = context;

    public override async Task ExecuteAsync()
    {
        Context.IsCompleted = false;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskTitle, "Kinnitamine");
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Pending);

        Context.Id = await TaskService.CreateWorkflowTaskAsync(Context);
    }

    public override async Task OnTaskChanged(IAlterTaskPayload payload)
    {
        var editor = payload.Storage.GetValueAs<string>(TaskStorageKeys.TaskEditor);
        var comment = payload.Storage.GetValueAs<string>(TaskStorageKeys.TaskComment);
        var delegatedTo = payload.Storage.GetValueAs<string>(TaskStorageKeys.TaskDelegatedTo);
        
        if (payload.Storage.TryGetValueAsEnum(TaskStorageKeys.TaskOutcome, out TaskOutcome? taskOutcome))
        {
            switch (taskOutcome)
            {
                case TaskOutcome.Approved:
                    await SetTaskCompleted(comment);
                    break;
                case TaskOutcome.Rejected:
                    await SetTaskRejected(comment);
                    break;
                case TaskOutcome.Delegated:
                    await SetTaskDelegated(comment, delegatedTo);
                    break;
            }
        }
    }

    private async Task SetTaskCompleted(string? comment)
    {
        Context.IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Approved);
            
        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskRejected(string? comment)
    {
        Context.IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Rejected);

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskDelegated(string? comment, string? delegatedTo)
    {
        Context.IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskDelegatedTo, delegatedTo);
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Delegated);

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }
}