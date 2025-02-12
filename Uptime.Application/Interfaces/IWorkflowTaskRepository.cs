using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowTaskRepository
{
    Task<WorkflowTaskContext?> GetWorkflowTaskContextAsync(TaskId taskId);
    Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask task);
    Task SaveWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus status);
}