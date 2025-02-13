﻿using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public abstract class UserTaskActivity(IWorkflowRepository repository, WorkflowTaskContext context) : IUserTaskActivity
{
    public WorkflowTaskContext Context => context;

    public bool IsCompleted { get; set; }

    public IUserTaskActivityData? TaskData { get; set; }
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Context.TaskGuid = Guid.NewGuid();

        InitializeContext();
        ExecuteTaskLogic();

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

    protected abstract void ExecuteTaskLogic();

    public abstract Task OnTaskChangedAsync(Dictionary<string, string?> storage, CancellationToken cancellationToken);
}