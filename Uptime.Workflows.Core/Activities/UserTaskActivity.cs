using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Services;

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
public abstract class UserTaskActivity(ITaskService taskService, IHistoryService historyService, 
    WorkflowTaskContext context) : IUserTaskActivity
{
    public WorkflowTaskContext Context => context;

    public bool IsCompleted { get; set; }

    public IUserTaskActivityData? TaskData { get; set; } // TODO: check why nullable

    protected virtual string? TaskCreatedHistoryDescription { get; set; }
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        InitializeContext();
        OnExecuteTask();

        PrincipalId principalId = PrincipalId.System;

        if (TaskData != null)
        {
            principalId = TaskData.AssignedByPrincipalId;
        }
        
        await historyService.CreateAsync(
            context.WorkflowId,
            WorkflowEventType.TaskCreated,
            principalId,
            description: TaskCreatedHistoryDescription,
            cancellationToken: cancellationToken
        );

        Context.TaskId = await taskService.CreateAsync(Context, cancellationToken);
    }

    protected virtual void InitializeContext()
    {
        if (TaskData != null)
        {
            Context.AssignedToPrincipalId = TaskData.AssignedToPrincipalId;
            Context.AssignedByPrincipalId = TaskData.AssignedByPrincipalId;
            Context.DueDate = TaskData.DueDate;
            Context.TaskDescription = TaskData.TaskDescription;
        }
    }

    public virtual async Task ChangedTaskAsync(PrincipalId executorId, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        await OnTaskChangedAsync(executorId, payload, cancellationToken);
        
        await taskService.UpdateAsync(Context, cancellationToken);
    }

    protected abstract void OnExecuteTask();

    protected abstract Task OnTaskChangedAsync(PrincipalId executorId, Dictionary<string, string?> payload, CancellationToken cancellationToken);
}