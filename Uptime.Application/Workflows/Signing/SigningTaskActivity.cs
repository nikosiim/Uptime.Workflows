using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Signing;

public class SigningTaskActivity(IWorkflowTaskRepository taskService, WorkflowTaskContext context)
    : UserTaskActivity(taskService, context)
{
    protected override void ExecuteTaskLogicAsync()
    {
        if (TaskData is null) return;

        Context.Storage.SetValueAsString(TaskStorageKeys.TaskTitle, "Allkirjastamine");
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Pending);
    }

    public override async Task OnTaskChanged(Dictionary<string, string?> storage)
    {
        string? editor = storage.GetValueAsString(TaskStorageKeys.TaskEditor);
        string? comment = storage.GetValueAsString(TaskStorageKeys.TaskComment);
        string? delegatedTo = storage.GetValueAsString(TaskStorageKeys.TaskDelegatedTo);
        
        if (storage.TryGetValueAsEnum(TaskStorageKeys.TaskOutcome, out TaskOutcome? taskOutcome))
        {
            await SetTaskOutcome(taskOutcome!.Value, editor, comment, delegatedTo);
        }
    }
    
    private async Task SetTaskOutcome(TaskOutcome outcome, string? editor, string? comment, string? delegatedTo = null)
    {
        IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskEditor, editor);
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, outcome);

        if (outcome == TaskOutcome.Delegated)
        {
            Context.Storage.SetValueAsString(TaskStorageKeys.TaskDelegatedTo, delegatedTo);
        }

        await TaskService.SaveWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }
}