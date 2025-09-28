using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace ApprovalWorkflow;

public class SigningTaskActivity(
    ITaskService taskService,
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    IWorkflowContext workflowContext)
    : UserTaskActivity(taskService, historyService, principalResolver, workflowContext)
{
    private readonly IPrincipalResolver _principalResolver = principalResolver;

    public bool IsTaskRejected { get; private set; }

    protected override void OnExecuteTask(IWorkflowActivityContext context, Principal assignedTo)
    {
        context.SetTaskTitle("Allkirjastamine");
        context.SetTaskOutcome("Ootel");

        TaskCreatedHistoryDescription = $"Tööülesanne {AssociationName} on loodud kasutajale {assignedTo.Name}";
    }

    protected override async Task OnTaskChangedAsync(WorkflowEventType action, IWorkflowActivityContext context,
        PrincipalSid executorSid, Dictionary<string, string?> payload, CancellationToken ct)
    {
        Principal executor = await _principalResolver.ResolveBySidAsync(executorSid, ct);
        Principal assignedTo = await _principalResolver.ResolveBySidAsync(context.AssignedToSid, ct);

        string? comment = payload.GetTaskComment();

        string outcome;
        string description;

        switch (action)
        {
            case WorkflowEventType.TaskRejected:
                IsTaskRejected = true;
                outcome = "Tagasilükatud";
                description = $"Kasutaja {executor.Name} on tööülesande {AssociationName} tagasi lükanud.";
                break;
            case WorkflowEventType.TaskCompleted:
                outcome = "Allkirjastatud";
                description = $"Kasutajale {assignedTo.Name} määratud tööülesanne on edukalt lõpetatud.";
                break;
            default:
                return;
        }

        IsCompleted = true;

        context.SetTaskStatus(WorkflowTaskStatus.Completed);
        context.SetTaskComment(comment);
        context.SetTaskOutcome(outcome);

        await HistoryService.CreateAsync(WorkflowId, action, executorSid, description: description, comment: comment, ct);
    }
}