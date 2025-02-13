using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalTaskActivity(IWorkflowRepository repository, WorkflowTaskContext context)
    : UserTaskActivity(repository, context)
{
    protected override void ExecuteTaskLogic()
    {
        if (TaskData is null) return;

        Context.Storage.SetValueAsString(TaskStorageKeys.TaskTitle, "Kinnitamine");
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Pending);
    }

    protected override void OnTaskChanged(Dictionary<string, string?> storage)
    {
        string? editor = storage.GetValueAsString(TaskStorageKeys.TaskEditor);
        string? comment = storage.GetValueAsString(TaskStorageKeys.TaskComment);
        string? delegatedTo = storage.GetValueAsString(TaskStorageKeys.TaskDelegatedTo);
        
        if (storage.TryGetValueAsEnum(TaskStorageKeys.TaskOutcome, out TaskOutcome? taskOutcome))
        {
            SetTaskOutcome(taskOutcome!.Value, editor, comment, delegatedTo);
        }
    }
    
    private void SetTaskOutcome(TaskOutcome outcome, string? editor, string? comment, string? delegatedTo = null)
    {
        IsCompleted = true;
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskEditor, editor);
        Context.Storage.SetValueAsString(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, outcome);

        if (outcome == TaskOutcome.Delegated)
        {
            Context.Storage.SetValueAsString(TaskStorageKeys.TaskDelegatedTo, delegatedTo);
        }

        Context.TaskStatus = WorkflowTaskStatus.Completed;
    }
}