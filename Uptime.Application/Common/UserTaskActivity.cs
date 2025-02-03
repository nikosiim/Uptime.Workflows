using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;

namespace Uptime.Application.Common;

public abstract class UserTaskActivity(ITaskService taskService) : IWorkflowActivity
{
    public ITaskService TaskService => taskService;
    public abstract Task ExecuteAsync();
    public abstract Task OnTaskChanged(AlterTaskPayload payload);
}