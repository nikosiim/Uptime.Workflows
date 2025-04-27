using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Services;
using static Uptime.Application.Constants;

namespace Uptime.Application.Workflows.Signing;

public class SigningTaskActivity(ITaskService taskService, IHistoryService historyService, WorkflowTaskContext context)
    : UserTaskActivity(taskService, historyService, context)
{
    private readonly IHistoryService _historyService = historyService;
    private string? AssociationName => Context.Storage.GetValueOrDefault(WorkflowStorageKeys.AssociationName);

    public bool IsTaskRejected { get; private set; }

    protected override void OnExecuteTask()
    {
        if (TaskData is null) return;

        Context.Storage.SetValue(TaskStorageKeys.TaskTitle, "Allkirjastamine");
        Context.Storage.SetValue(TaskStorageKeys.TaskOutcome, "Ootel");

        TaskCreatedHistoryDescription = $"Tööülesanne {AssociationName} on loodud kasutajale {TaskData.AssignedTo}";
    }
    
    protected override async Task OnTaskChangedAsync(Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        string? author = payload.GetValue(TaskStorageKeys.TaskEditor);
        string? comment = payload.GetValue(TaskStorageKeys.TaskComment);
        
        if (payload.TryGetValueAsEnum(TaskStorageKeys.TaskResult, out WorkflowEventType workflowEvent))
        {
            string outcome;
            string description;

            switch (workflowEvent)
            {
                case WorkflowEventType.TaskRejected:
                    IsTaskRejected = true;
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
            
            await _historyService.CreateAsync(Context.WorkflowId, workflowEvent, author, description:description, comment:comment, cancellationToken);
        }
    }
}