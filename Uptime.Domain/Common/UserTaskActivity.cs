using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public abstract class UserTaskActivity(IWorkflowTaskRepository taskService, WorkflowTaskContext context) : IUserTaskActivity
{
    public WorkflowTaskContext Context => context;
    public IWorkflowTaskRepository TaskService => taskService;
    public WorkflowId WorkflowId => context.WorkflowId;

    public bool IsCompleted { get; set; }
    public IUserTaskActivityData? TaskData { get; set; }

    public abstract Task OnTaskChanged(Dictionary<string, string?> storage);
    protected abstract void ExecuteTaskLogicAsync();

    public async Task ExecuteAsync()
    {
        Context.TaskGuid = Guid.NewGuid();

        InitializeContext();
        ExecuteTaskLogicAsync();

        Context.TaskId = await TaskService.CreateWorkflowTaskAsync(Context);
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
}