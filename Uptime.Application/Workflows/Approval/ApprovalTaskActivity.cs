using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Choices;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalTaskActivity(IWorkflowRepository repository, WorkflowTaskContext context)
    : UserTaskActivity(repository, context)
{
    protected override void ExecuteTaskLogic()
    {
        if (TaskData is null) return;

        Context.Storage.SetValue(TaskStorageKeys.TaskTitle, "Kinnitamine");
        Context.Storage.SetValue(TaskStorageKeys.TaskOutcome, TaskOutcome.Pending);
    }

    protected override void OnTaskChanged(Dictionary<string, string?> storage)
    {
        string? editor = storage.GetValue(TaskStorageKeys.TaskEditor);
        string? comment = storage.GetValue(TaskStorageKeys.TaskComment);
        string? delegatedTo = storage.GetValue(TaskStorageKeys.TaskDelegatedTo);
        
        if (storage.TryGetValue(TaskStorageKeys.TaskOutcome, out string? taskOutcome))
        {
            SetTaskOutcome(taskOutcome!, editor, comment, delegatedTo);
        }
    }
    
    private void SetTaskOutcome(string outcome, string? editor, string? comment, string? delegatedTo = null)
    {
        IsCompleted = true;
        Context.Storage.SetValue(TaskStorageKeys.TaskEditor, editor);
        Context.Storage.SetValue(TaskStorageKeys.TaskComment, comment);
        Context.Storage.SetValue(TaskStorageKeys.TaskOutcome, outcome);

        if (outcome == TaskOutcome.Delegated)
        {
            Context.Storage.SetValue(TaskStorageKeys.TaskDelegatedTo, delegatedTo);
        }

        Context.TaskStatus = WorkflowTaskStatus.Completed;
    }
}