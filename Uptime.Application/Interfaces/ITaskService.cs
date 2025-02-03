using Uptime.Shared.Enums;

namespace Uptime.Application.Interfaces;

public interface ITaskService
{
    Task<int> CreateWorkflowTaskAsync(IWorkflowTask task);
    Task UpdateWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus status);
}