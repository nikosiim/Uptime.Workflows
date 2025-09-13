using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Services;
using static SigningWorkflow.Constants;

namespace SigningWorkflow;

public class SigningTaskActivity(ITaskService taskService, IHistoryService historyService, WorkflowTaskContext context)
    : UserTaskActivity(taskService, historyService, context)
{
    private readonly IHistoryService _historyService = historyService;
    private string? AssociationName => Context.Storage.GetValueOrDefault(TaskStorageKeys.AssociationName); // TODO: check what setting of AssociationName

    public bool IsTaskRejected { get; private set; }

    protected override void OnExecuteTask()
    {
        if (TaskData is null) return;

        Context.Storage.SetValue(TaskStorageKeys.TaskTitle, "Allkirjastamine");
        Context.Storage.SetValue(TaskStorageKeys.TaskOutcome, "Ootel");

        TaskCreatedHistoryDescription = $"Tööülesanne {AssociationName} on loodud kasutajale {TaskData.AssignedToPrincipalId}";
    }
    
    protected override async Task OnTaskChangedAsync(PrincipalId executorId, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
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
                    description = $"Kasutaja {TaskData?.AssignedToPrincipalId} on tööülesande {AssociationName} tagasilükanud.";
                    break;
                case WorkflowEventType.TaskCompleted:
                    outcome = "Allkirjastatud";
                    description = $"Kasutajale {TaskData?.AssignedToPrincipalId} määratud tööülesanne on edukalt lõpetatud.";
                    break;
                default:
                    return;
            }
            
            IsCompleted = true;
            Context.TaskStatus = WorkflowTaskStatus.Completed;
            Context.Storage.SetValue(TaskStorageKeys.TaskComment, comment);
            Context.Storage.SetValue(TaskStorageKeys.TaskOutcome, outcome);
            
            await _historyService.CreateAsync(Context.WorkflowId, workflowEvent, executorId, description:description, comment:comment, cancellationToken);
        }
    }
}