using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

/// <summary>
/// Base class for defining a single user task inside a workflow.
/// 
/// Responsibilities:
/// • Creates a task in the system and records it in history.
/// • Stores who the task is assigned to, by whom, due dates, etc.
/// • Provides <c>ChangedTaskAsync</c> to handle updates (approval,
///   rejection, delegation, etc.).
/// 
/// New developers:
/// Derive from this class when you want to create a **specific kind of
/// user task** (e.g. ApprovalTaskActivity, SigningTaskActivity).
/// Override <c>OnExecuteTask</c> to define what happens when the task
/// is created, and <c>OnTaskChangedAsync</c> to handle user responses.
/// 
/// Think of this as the “unit of work” a human needs to perform within
/// a workflow.
/// </summary>
public abstract class UserTaskActivity(
    ITaskService taskService, 
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    IWorkflowContext workflowContext) : IUserTaskActivity 
{
    protected readonly IHistoryService HistoryService = historyService;
    protected string? AssociationName => workflowContext.GetAssociationName();
    protected WorkflowId WorkflowId => workflowContext.GetWorkflowId();

    public Guid TaskGuid { get; private set; }
    public bool IsCompleted { get; protected set; }
    
    protected virtual string? TaskCreatedHistoryDescription { get; set; }
    
    public async Task ExecuteAsync(IWorkflowActivityContext context, CancellationToken ct)
    {
        TaskGuid = context.TaskGuid;

        Principal initiator = await principalResolver.ResolveBySidAsync(workflowContext.GetInitiatorSid(), ct);
        Principal assignedTo = await principalResolver.ResolveBySidAsync(context.AssignedToSid, ct);

        OnExecuteTask(context, assignedTo);
        
        await HistoryService.CreateAsync(
            WorkflowId,
            WorkflowEventType.TaskCreated,
            initiator.Sid,
            description: TaskCreatedHistoryDescription,
            ct: ct
        );

        await taskService.CreateAsync(WorkflowId, initiator.Id, assignedTo.Id, context, ct);
    }

    public virtual async Task ChangedTaskAsync(WorkflowEventType action, IWorkflowActivityContext context, 
        PrincipalSid executorSid, Dictionary<string, string?> payload, CancellationToken ct)
    {
        TaskGuid = context.TaskGuid;

        await OnTaskChangedAsync(action, context, executorSid, payload, ct);
        await taskService.UpdateAsync(context, ct);
    }

    protected abstract void OnExecuteTask(IWorkflowActivityContext context, Principal assignedTo);

    protected abstract Task OnTaskChangedAsync(WorkflowEventType action, IWorkflowActivityContext context, 
        PrincipalSid executorSid, Dictionary<string, string?> payload, CancellationToken ct);
}