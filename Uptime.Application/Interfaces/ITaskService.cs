using Uptime.Domain.Enums;

namespace Uptime.Application.Interfaces;

public interface ITaskService
{
    Task<int> CreateWorkflowTaskAsync(IWorkflowTask task);
    Task UpdateWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus status);
}