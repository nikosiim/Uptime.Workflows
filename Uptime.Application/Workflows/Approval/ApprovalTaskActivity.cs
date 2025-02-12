using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalTaskActivity(IWorkflowTaskRepository taskService, WorkflowTaskContext context)
    : UserTaskActivity(taskService, context)
{
    protected override void ExecuteTaskLogic()
    {
        if (TaskData is null) return;

        Context.Storage.SetValueAsString(TaskStorageKeys.TaskTitle, "Kinnitamine");
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Pending);
    }

    public override async Task OnTaskChangedAsync(Dictionary<string, string?> storage, CancellationToken cancellationToken)
    {
        string? editor = storage.GetValueAsString(TaskStorageKeys.TaskEditor);
        string? comment = storage.GetValueAsString(TaskStorageKeys.TaskComment);
        string? delegatedTo = storage.GetValueAsString(TaskStorageKeys.TaskDelegatedTo);
        
        if (storage.TryGetValueAsEnum(TaskStorageKeys.TaskOutcome, out TaskOutcome? taskOutcome))
        {
            await SetTaskOutcomeAsync(taskOutcome!.Value, editor, comment, delegatedTo, cancellationToken);
        }
    }
    
    private async Task SetTaskOutcomeAsync(TaskOutcome outcome, string? editor, string? comment, string? delegatedTo = null, CancellationToken cancellationToken = default)
    {
        IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskEditor, editor);
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, outcome);

        if (outcome == TaskOutcome.Delegated)
        {
            Context.Storage.SetValueAsString(TaskStorageKeys.TaskDelegatedTo, delegatedTo);
        }

        await TaskService.SaveWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed, cancellationToken);
    }
}