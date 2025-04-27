using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Core;

public abstract class UserTaskActivity(ITaskService taskService, IHistoryService historyService, WorkflowTaskContext context) : IUserTaskActivity
{
    public WorkflowTaskContext Context => context;

    public bool IsCompleted { get; set; }

    public IUserTaskActivityData? TaskData { get; set; }

    protected virtual string? TaskCreatedHistoryDescription { get; set; }
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        InitializeContext();
        OnExecuteTask();

        await historyService.CreateAsync(
            context.WorkflowId,
            WorkflowEventType.TaskCreated,
            TaskData?.AssignedBy,
            description: TaskCreatedHistoryDescription,
            cancellationToken: cancellationToken
        );

        Context.TaskId = await taskService.CreateAsync(Context, cancellationToken);
    }

    protected virtual void InitializeContext()
    {
        if (TaskData != null)
        {
            Context.AssignedTo = TaskData.AssignedTo;
            Context.AssignedBy = TaskData.AssignedBy;
            Context.DueDate = TaskData.DueDate;
            Context.TaskDescription = TaskData.TaskDescription;
        }
    }

    public virtual async Task ChangedTaskAsync(Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        await OnTaskChangedAsync(payload, cancellationToken);
        
        await taskService.UpdateAsync(Context, cancellationToken);
    }

    protected abstract void OnExecuteTask();
    protected abstract Task OnTaskChangedAsync(Dictionary<string, string?> payload, CancellationToken cancellationToken);
}