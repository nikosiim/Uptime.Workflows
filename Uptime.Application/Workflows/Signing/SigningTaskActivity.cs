﻿using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Extensions;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Signing;

public class SigningTaskActivity(IWorkflowRepository repository, WorkflowTaskContext context)
    : UserTaskActivity(repository, context)
{
    private readonly IWorkflowRepository _repository = repository;
    private string? AssociationName => Context.Storage.GetValueOrDefault(WorkflowStorageKeys.AssociationName);

    protected override void OnExecuteTask()
    {
        if (TaskData is null) return;

        Context.Storage.SetValue(TaskStorageKeys.TaskTitle, "Allkirjastamine");
        Context.Storage.SetValue(TaskStorageKeys.TaskOutcome, "Ootel");

        TaskCreatedHistoryDescription = $"Tööülesanne {AssociationName} on loodud kasutajale {TaskData.AssignedTo}";
    }
    
    protected override async Task OnTaskChangedAsync(Dictionary<string, string?> input, CancellationToken cancellationToken)
    {
        string? author = input.GetValue(TaskStorageKeys.TaskEditor);
        string? comment = input.GetValue(TaskStorageKeys.TaskComment);
        
        if (input.TryGetValueAsEnum(TaskStorageKeys.TaskResult, out WorkflowEventType workflowEvent))
        {
            string outcome;
            string description;

            switch (workflowEvent)
            {
                case WorkflowEventType.TaskRejected:
                    outcome = "Tagasilükatud";
                    description = $"Kasutaja {TaskData?.AssignedTo} on tööülesande {AssociationName} tagasilükanud.";
                    break;
                case WorkflowEventType.TaskCompleted:
                    outcome = "Allkirjastatud";
                    description = $"Kasutajale {TaskData?.AssignedTo} määratud tööülesanne on edukalt lõpetatud.";
                    break;
                default:
                    return;
            }
            
            IsCompleted = true;
            Context.TaskStatus = WorkflowTaskStatus.Completed;

            Context.Storage.SetValue(TaskStorageKeys.TaskEditor, author);
            Context.Storage.SetValue(TaskStorageKeys.TaskComment, comment);
            Context.Storage.SetValue(TaskStorageKeys.TaskOutcome, outcome);
            
            await _repository.AddWorkflowHistoryAsync(Context.WorkflowId, workflowEvent, author, description:description, comment:comment, cancellationToken);
        }
    }
}