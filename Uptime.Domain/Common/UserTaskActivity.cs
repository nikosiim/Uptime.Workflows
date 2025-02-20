using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public abstract class UserTaskActivity(IWorkflowRepository repository, WorkflowTaskContext context) : IUserTaskActivity
{
    public WorkflowTaskContext Context => context;

    public bool IsCompleted { get; set; }

    public IUserTaskActivityData? TaskData { get; set; }

    protected virtual string? TaskCreatedHistoryDescription { get; set; }
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Context.TaskGuid = Guid.NewGuid();

        InitializeContext();
        OnExecuteTask();

        await repository.AddWorkflowHistoryAsync(
            context.WorkflowId,
            WorkflowEventType.TaskCreated,
            TaskData?.AssignedBy,
            description: TaskCreatedHistoryDescription,
            cancellationToken: cancellationToken
        );

        Context.TaskId = await repository.CreateWorkflowTaskAsync(Context, cancellationToken);
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
        
        await repository.SaveWorkflowTaskAsync(Context, cancellationToken);
    }

    protected abstract void OnExecuteTask();
    protected abstract Task OnTaskChangedAsync(Dictionary<string, string?> input, CancellationToken cancellationToken);
}