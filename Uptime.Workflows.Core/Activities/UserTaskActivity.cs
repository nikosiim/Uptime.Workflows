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
    IWorkflowActivityContext activityContext, 
    IWorkflowContext workflowContext) : IUserTaskActivity 
{
    public IWorkflowActivityContext Context => activityContext;
    
    protected readonly IHistoryService HistoryService = historyService;
    protected string? AssociationName => workflowContext.GetAssociationName();
    protected WorkflowId WorkflowId => workflowContext.GetWorkflowId();

    protected TaskId TaskId
    {
        get => Context.GetTaskId();
        private set => Context.SetTaskId(value);
    }

    public bool IsCompleted { get; protected set; }
    
    protected virtual string? TaskCreatedHistoryDescription { get; set; }
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        OnExecuteTask();
        
        await HistoryService.CreateAsync(
            WorkflowId,
            WorkflowEventType.TaskCreated,
            activityContext.AssignedByPrincipalId,
            description: TaskCreatedHistoryDescription,
            cancellationToken: cancellationToken
        );

        TaskId = await taskService.CreateAsync(WorkflowId, Context, cancellationToken);
    }

    public virtual async Task ChangedTaskAsync(WorkflowEventType action, Principal executedBy, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        await OnTaskChangedAsync(action, executedBy, payload, cancellationToken);
        
        await taskService.UpdateAsync(Context, cancellationToken);
    }

    protected abstract void OnExecuteTask();

    protected abstract Task OnTaskChangedAsync(WorkflowEventType action, Principal executedBy, Dictionary<string, string?> payload, CancellationToken ct);
}