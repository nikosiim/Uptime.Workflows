﻿using Uptime.Application.Common;
using Uptime.Application.Interfaces;
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
        Context.Storage[TaskStorageKeys.TaskTitle] = "Kinnitamine";
        Context.Storage[TaskStorageKeys.TaskOutcome] = TaskOutcome.Pending;
        Context.Id = await TaskService.CreateWorkflowTaskAsync(Context);
    }

    public override async Task OnTaskChanged(IAlterTaskPayload payload)
    {
        var editor = payload.Storage.TryGetValueAs<string>(TaskStorageKeys.TaskEditor);
        var delegatedTo = payload.Storage.TryGetValueAs<string>(TaskStorageKeys.TaskDelegatedTo);

        var comment = payload.Storage.TryGetValueAs<string>(TaskStorageKeys.TaskComment);
        var outcome = payload.Storage.TryGetValueAs<TaskOutcome?>(TaskStorageKeys.TaskOutcome);

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
                    await SetTaskDelegated(comment, delegatedTo);
                    break;
            }
        }
    }

    private async Task SetTaskCompleted(string? comment)
    {
        Context.IsCompleted = true;
        Context.Storage[TaskStorageKeys.TaskOutcome] = TaskOutcome.Approved;
        Context.Storage[TaskStorageKeys.TaskComment] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskRejected(string? comment)
    {
        Context.IsCompleted = true;
        Context.Storage[TaskStorageKeys.TaskOutcome] = TaskOutcome.Rejected;
        Context.Storage[TaskStorageKeys.TaskComment] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }

    private async Task SetTaskDelegated(string? comment, string? delegatedTo)
    {
        Context.IsCompleted = true;
        Context.Storage[TaskStorageKeys.TaskDelegatedTo] = delegatedTo;
        Context.Storage[TaskStorageKeys.TaskOutcome] = TaskOutcome.Delegated;
        Context.Storage[TaskStorageKeys.TaskComment] = comment;

        await TaskService.UpdateWorkflowTaskAsync(Context, WorkflowTaskStatus.Completed);
    }
}