using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowTaskRepository
{
    Task<WorkflowTaskContext?> GetWorkflowTaskContextAsync(TaskId taskId);
    Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask task);
    Task SaveWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus status);
}