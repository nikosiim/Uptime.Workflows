using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowTaskRepository
{
    Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask task, CancellationToken cancellationToken);
    Task SaveWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus status, CancellationToken cancellationToken);
}