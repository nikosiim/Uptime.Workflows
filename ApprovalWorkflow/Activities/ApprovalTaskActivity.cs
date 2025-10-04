using Microsoft.Extensions.Logging;
using Workflows.Core.Activities;
using Workflows.Core.Common;
using Workflows.Core.Enums;
using Workflows.Core.Extensions;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace ApprovalWorkflow;

public sealed class ApprovalTaskActivity(
    ITaskService taskService,
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    IWorkflowContext workflowContext, 
    ILogger<ApprovalTaskActivity> logger)
    : UserTaskActivity(taskService, historyService, principalResolver, workflowContext)
{
    private readonly IPrincipalResolver _principalResolver = principalResolver;
    
    public bool IsTaskRejected { get; private set; }
    public Principal? TaskDelegatedToPrincipal { get; private set; }

    protected override void OnExecuteTask(IWorkflowActivityContext context, Principal assignedTo)
    {
        context.SetTaskTitle("Kinnitamine");
        context.SetTaskOutcome("Ootel");

        TaskCreatedHistoryDescription = $"Tööülesanne {AssociationName} on loodud kasutajale {assignedTo.Name}";
    }

    protected override async Task OnTaskChangedAsync(WorkflowEventType action, IWorkflowActivityContext context, 
        PrincipalSid executorSid, Dictionary<string, string?> payload, CancellationToken ct)
    {
        string? comment = payload.GetTaskComment();

        Principal executor = await _principalResolver.ResolveBySidAsync(executorSid, ct);
        Principal assignedTo = await _principalResolver.ResolveBySidAsync(context.AssignedToSid, ct);

        switch (action)
        {
            case WorkflowEventType.TaskRejected:
                await HandleTaskRejectedAsync(context, executor, comment, ct);
                break;
            case WorkflowEventType.TaskDelegated:
                await HandleTaskDelegatedAsync(context, executor, payload, comment, ct);
                break;
            case WorkflowEventType.TaskCancelled:
                await HandleTaskCancelledAsync(context, executor, assignedTo, comment, ct);
                break;
            case WorkflowEventType.TaskCompleted:
                await HandleTaskCompletedAsync(context, executor, assignedTo, comment, ct);
                break;
            default:
                return;
        }
    }

    private async Task HandleTaskRejectedAsync(IWorkflowActivityContext context, Principal executor, string? comment, CancellationToken ct)
    {
        IsTaskRejected = true;
        IsCompleted = true;

        context.SetTaskStatus(WorkflowTaskStatus.Completed);
        context.SetTaskComment(comment);
        context.SetTaskOutcome("Tagasilükatud");

        await HistoryService.CreateAsync(
            WorkflowId, 
            WorkflowEventType.TaskRejected, 
            executor.Sid,
            description: $"Kasutaja {executor.Name} on tööülesande {AssociationName} tagasilükanud.", 
            comment: comment, 
            ct);
    }

    private async Task HandleTaskDelegatedAsync(IWorkflowActivityContext context, Principal executor, 
        Dictionary<string, string?> payload, string? comment, CancellationToken ct)
    {
        string? sid = payload.GetTaskDelegatedToSid();
        if (string.IsNullOrWhiteSpace(sid))
        {
            logger.LogWarning("Task {TaskGuid} DelegatedTo SID value not provided", context.TaskGuid);
            return;
        }

        TaskDelegatedToPrincipal = await _principalResolver.ResolveBySidAsync((PrincipalSid)sid, ct);
      
        await HistoryService.CreateAsync(
            WorkflowId, 
            WorkflowEventType.TaskDelegated, 
            executor.Sid,
            description: $"Tööülesanne {AssociationName} on suunatud kasutajale {TaskDelegatedToPrincipal.Name}", 
            comment: comment, 
            ct);

        IsCompleted = true;

        context.SetTaskStatus(WorkflowTaskStatus.Completed);
        context.SetTaskComment(comment);
        context.SetTaskOutcome("Suunatud");
    }

    private async Task HandleTaskCancelledAsync(IWorkflowActivityContext context, Principal executor, Principal assignedTo,
        string? comment, CancellationToken ct)
    {
        IsCompleted = true;

        context.SetTaskStatus(WorkflowTaskStatus.Completed);
        context.SetTaskComment(comment);
        context.SetTaskOutcome("Tühistatud");

        await HistoryService.CreateAsync(
            WorkflowId,
            WorkflowEventType.TaskCancelled,
            executor.Sid,
            description: $"Kasutaja {assignedTo.Name} on tööülesande {AssociationName} tühistanud.",
            comment: comment,
            ct);
    }

    private async Task HandleTaskCompletedAsync(IWorkflowActivityContext context, Principal executor, Principal assignedTo,
        string? comment, CancellationToken ct)
    {
        IsCompleted = true;

        context.SetTaskStatus(WorkflowTaskStatus.Completed);
        context.SetTaskComment(comment);
        context.SetTaskOutcome("Kinnitatud");

        await HistoryService.CreateAsync(
            WorkflowId,
            WorkflowEventType.TaskCompleted,
            executor.Sid,
            description: $"Kasutajale {assignedTo.Name} määratud tööülesanne on edukalt lõpetatud.",
            comment: comment,
            ct);
    }
}