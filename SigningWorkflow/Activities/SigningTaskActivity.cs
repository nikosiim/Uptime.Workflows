using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace SigningWorkflow;

public class SigningTaskActivity(ITaskService taskService, IHistoryService historyService, IWorkflowActivityContext activityContext, IWorkflowContext workflowContext)
    : UserTaskActivity(taskService, historyService, activityContext, workflowContext)
{
    public bool IsTaskRejected { get; private set; }

    protected override void OnExecuteTask()
    {
        Context.SetTaskTitle("Allkirjastamine");
        Context.SetTaskOutcome("Ootel");

        TaskCreatedHistoryDescription = $"Tööülesanne {AssociationName} on loodud kasutajale {Context.AssignedToPrincipalId}";
    }

    protected override async Task OnTaskChangedAsync(WorkflowEventType action, Principal executedBy, Dictionary<string, string?> payload, CancellationToken ct)
    {
        string? comment = payload.GetTaskComment();

        string outcome;
        string description;

        switch (action)
        {
            case WorkflowEventType.TaskRejected:
                IsTaskRejected = true;
                outcome = "Tagasilükatud";
                description = $"Kasutaja {executedBy.Id} on tööülesande {AssociationName} tagasi lükanud.";
                break;
            case WorkflowEventType.TaskCompleted:
                outcome = "Allkirjastatud";
                description = $"Kasutajale {Context.AssignedToPrincipalId} määratud tööülesanne on edukalt lõpetatud.";
                break;
            default:
                return;
        }

        IsCompleted = true;

        Context.SetTaskStatus(WorkflowTaskStatus.Completed);
        Context.SetTaskComment(comment);
        Context.SetTaskOutcome(outcome);

        await HistoryService.CreateAsync(WorkflowId, action, executedBy.Id, description: description, comment: comment, ct);
    }
}