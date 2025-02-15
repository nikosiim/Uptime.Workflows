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
        ExecuteTaskLogic();

        await repository.AddWorkflowHistoryAsync(
            context.WorkflowId,
            WorkflowHistoryEventType.TaskCreated,
            TaskData?.AssignedBy,
            outcome: null,
            description: TaskCreatedHistoryDescription,
            cancellationToken
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

    public virtual async Task OnTaskChangedAsync(Dictionary<string, string?> storage, CancellationToken cancellationToken)
    {
        OnTaskChanged(storage);

        await repository.SaveWorkflowTaskAsync(Context, cancellationToken);
    }

    protected abstract void ExecuteTaskLogic();
    protected abstract void OnTaskChanged(Dictionary<string, string?> storage);
}