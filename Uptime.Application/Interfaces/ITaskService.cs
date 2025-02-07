using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Application.Interfaces;

public interface ITaskService
{
    Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask task);
    Task UpdateWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus status);
}