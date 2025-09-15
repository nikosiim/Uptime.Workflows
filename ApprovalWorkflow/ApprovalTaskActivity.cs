using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;
using static Uptime.Workflows.Core.TaskInputPayloadExtensions;

namespace ApprovalWorkflow;

public sealed class ApprovalTaskActivity(
    ITaskService taskService,
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    IWorkflowTaskContext taskContext,
    IWorkflowContext workflowContext)
    : UserTaskActivity(taskService, historyService, taskContext, workflowContext)
{
    public Principal? TaskDelegatedToPrincipal { get; private set; }
    public bool IsTaskRejected { get; private set; }

    protected override void OnExecuteTask()
    {
        Context.SetTaskTitle("Kinnitamine");
        Context.SetTaskOutcome("Ootel");

        TaskCreatedHistoryDescription = $"Tööülesanne {AssociationName} on loodud kasutajale {Context.AssignedToPrincipalId}";
    }

    protected override async Task OnTaskChangedAsync(Principal executedBy, Dictionary<string, string?> payload, CancellationToken ct)
    {
        if (!payload.TryGetValueAsEnum(TaskInputKeys.TaskResult, out WorkflowEventType workflowEvent))
            return;
        
        string? comment = payload.GetTaskComment();

        switch (workflowEvent)
        {
            case WorkflowEventType.TaskRejected:
                await HandleTaskRejectedAsync(executedBy, comment, ct);
                break;
            case WorkflowEventType.TaskDelegated:
                await HandleTaskDelegatedAsync(executedBy, payload, comment, ct);
                break;
            case WorkflowEventType.TaskCancelled:
                await HandleTaskCancelledAsync(executedBy, comment, ct);
                break;
            case WorkflowEventType.TaskCompleted:
                await HandleTaskCompletedAsync(executedBy, comment, ct);
                break;
            default:
                return;
        }
    }

    private async Task HandleTaskRejectedAsync(Principal executedBy, string? comment, CancellationToken ct)
    {
        IsTaskRejected = true;
        IsCompleted = true;

        Context.SetTaskStatus(WorkflowTaskStatus.Completed);
        Context.SetTaskComment(comment);
        Context.SetTaskOutcome("Tagasilükatud");

        await HistoryService.CreateAsync(
            WorkflowId, 
            WorkflowEventType.TaskRejected, 
            executedBy.Id,
            description: $"Kasutaja {executedBy.Id} on tööülesande {AssociationName} tagasilükanud.", 
            comment: comment, 
            ct);
    }

    private async Task HandleTaskDelegatedAsync(Principal executedBy, Dictionary<string, string?> payload, string? comment, CancellationToken ct)
    {
        IsCompleted = true;

        Context.SetTaskStatus(WorkflowTaskStatus.Completed);
        Context.SetTaskComment(comment);
        Context.SetTaskOutcome("Suunatud");

        string? sid = payload.GetTaskDelegatedToSid();
        TaskDelegatedToPrincipal = await WorkflowPrincipalResolver.ResolvePrincipalBySidAsync(principalResolver, sid, ct);
      
        await HistoryService.CreateAsync(
            WorkflowId, 
            WorkflowEventType.TaskDelegated, 
            executedBy.Id,
            description: $"Tööülesanne {AssociationName} on suunatud kasutajale {sid}", 
            comment: comment, 
            ct);
    }

    private async Task HandleTaskCancelledAsync(Principal executedBy, string? comment, CancellationToken ct)
    {
        IsCompleted = true;

        Context.SetTaskStatus(WorkflowTaskStatus.Completed);
        Context.SetTaskComment(comment);
        Context.SetTaskOutcome("Tühistatud");

        await HistoryService.CreateAsync(
            WorkflowId,
            WorkflowEventType.TaskCancelled,
            executedBy.Id,
            description: $"Kasutaja {Context.AssignedToPrincipalId} on tööülesande {AssociationName} tühistanud.",
            comment: comment,
            ct);
    }

    private async Task HandleTaskCompletedAsync(Principal executedBy, string? comment, CancellationToken ct)
    {
        IsCompleted = true;

        Context.SetTaskStatus(WorkflowTaskStatus.Completed);
        Context.SetTaskComment(comment);
        Context.SetTaskOutcome("Kinnitatud");

        await HistoryService.CreateAsync(
            WorkflowId,
            WorkflowEventType.TaskCompleted,
            executedBy.Id,
            description: $"Kasutajale {Context.AssignedToPrincipalId} määratud tööülesanne on edukalt lõpetatud.",
            comment: comment,
            ct);
    }
}