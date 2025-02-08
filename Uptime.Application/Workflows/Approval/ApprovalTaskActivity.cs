using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalTaskActivity(ITaskService taskService, WorkflowTaskContext context)
    : UserTaskActivity(taskService, context), IWorkflowActivity
{
    public ApprovalTaskData? InitiationData { get; set; }

    public override async Task ExecuteAsync()
    {
        IsCompleted = false;

        if (InitiationData != null)
        {
            Context.AssignedTo = InitiationData.AssignedTo;
            Context.AssignedBy = InitiationData.AssignedBy;
            Context.DueDate = InitiationData.DueDate;
            Context.TaskDescription = InitiationData.TaskDescription;
            Context.Storage.SetValueAsString(TaskStorageKeys.TaskTitle, "Kinnitamine");
            Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Pending);

            Context.TaskId = await TaskService.CreateWorkflowTaskAsync(Context);
        }
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
                    await SetTaskCompleted(editor, comment);
                    break;
                case TaskOutcome.Rejected:
                    await SetTaskRejected(editor, comment);
                    break;
                case TaskOutcome.Delegated:
                    await SetTaskDelegated(editor, comment, delegatedTo);
                    break;
            }
        }
    }

    private async Task SetTaskCompleted(string? editor, string? comment)
    {
        IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskEditor, editor);
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Approved);
            
        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskRejected(string? editor, string? comment)
    {
        IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskEditor, editor);
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Rejected);

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskDelegated(string? editor, string? comment, string? delegatedTo)
    {
        IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskEditor, editor);
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskDelegatedTo, delegatedTo);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Delegated);

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }
}