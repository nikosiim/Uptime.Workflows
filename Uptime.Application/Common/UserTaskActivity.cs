using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Common;

public abstract class UserTaskActivity(ITaskService taskService, WorkflowTaskContext context) : IWorkflowActivity
{
    public ITaskService TaskService => taskService;
    public WorkflowId WorkflowId => context.WorkflowId;
    public WorkflowTaskContext Context => context;
    public bool IsCompleted { get; set; }
    public abstract Task ExecuteAsync();
    public abstract Task OnTaskChanged(IAlterTaskPayload payload);
}