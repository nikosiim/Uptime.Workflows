using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Application.Interfaces;

public interface ITaskService
{
    Task<WorkflowTaskContext?> GetWorkflowTaskContextAsync(TaskId taskId);
    Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask task);
    Task UpdateWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus status);
}