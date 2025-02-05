using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public abstract class UserTaskActivity(ITaskService taskService) : IWorkflowActivity
{
    public ITaskService TaskService => taskService;
    public abstract Task ExecuteAsync();
    public abstract Task OnTaskChanged(IAlterTaskPayload payload);
}