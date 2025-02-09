using Uptime.Application.Common;
using Uptime.Application.Interfaces;
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
        if (InitiationData is null) return;

        InitializeContext(InitiationData);

        Context.Storage.SetValueAsString(TaskStorageKeys.TaskTitle, "Kinnitamine");
        Context.Storage.SetValueAsEnum<TaskOutcome>(TaskStorageKeys.TaskOutcome, TaskOutcome.Pending);

        Context.TaskId = await TaskService.CreateWorkflowTaskAsync(Context);
    }

    private void InitializeContext(ApprovalTaskData data)
    {
        Context.AssignedTo = data.AssignedTo;
        Context.AssignedBy = data.AssignedBy;
        Context.DueDate = data.DueDate;
        Context.TaskDescription = data.TaskDescription;
    }

    public override async Task OnTaskChanged(IAlterTaskPayload payload)
    {
        string? editor = payload.Storage.GetValueAsString(TaskStorageKeys.TaskEditor);
        string? comment = payload.Storage.GetValueAsString(TaskStorageKeys.TaskComment);
        string? delegatedTo = payload.Storage.GetValueAsString(TaskStorageKeys.TaskDelegatedTo);

        Context.TaskId = payload.TaskId;// TODO: miks siin contexti peab täitma, millegipärast on väärtused siit puudu

        if (payload.Storage.TryGetValueAsEnum(TaskStorageKeys.TaskOutcome, out TaskOutcome? taskOutcome))
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

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }
}