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
        string? comment = payload.GetTaskComment();

        if (payload.TryGetValueAsEnum(TaskInputKeys.TaskResult, out WorkflowEventType workflowEvent))
        {
            string outcome;
            string description;

            switch (workflowEvent)
            {
                case WorkflowEventType.TaskRejected:
                    IsTaskRejected = true;
                    outcome = "Tagasilükatud";
                    description = $"Kasutaja {executedBy.Id} on tööülesande {AssociationName} tagasilükanud.";
                    break;
                case WorkflowEventType.TaskDelegated:
                    string? sid = payload.GetTaskDelegatedToSid();
                    if (string.IsNullOrWhiteSpace(sid))
                        throw new WorkflowValidationException(ErrorCode.NotFound, "DelegatedTo SID not provided");
                    TaskDelegatedToPrincipal = await principalResolver.ResolveBySidAsync(sid, ct);
                    if (TaskDelegatedToPrincipal == null)
                        throw new WorkflowValidationException(ErrorCode.NotFound, $"Not found for SID: {sid}");
                    outcome = "Suunatud";
                    description = $"Tööülesanne {AssociationName} on suunatud kasutajale {sid}";
                    break;
                case WorkflowEventType.TaskCancelled:
                    outcome = "Tühistatud";
                    description = $"Kasutaja {Context.AssignedToPrincipalId} on tööülesande {AssociationName} tühistanud.";
                    break;
                case WorkflowEventType.TaskCompleted:
                    outcome = "Kinnitatud";
                    description = $"Kasutajale {Context.AssignedToPrincipalId} määratud tööülesanne on edukalt lõpetatud.";
                    break;
                default:
                    return;
            }

            IsCompleted = true;

            Context.SetTaskStatus(WorkflowTaskStatus.Completed);
            Context.SetTaskComment(comment);
            Context.SetTaskOutcome(outcome);
            
            await HistoryService.CreateAsync(WorkflowId, workflowEvent, executedBy.Id, description:description, comment:comment, ct);
        }
    }
}