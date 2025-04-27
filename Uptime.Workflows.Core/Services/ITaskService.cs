using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Services;

public interface ITaskService
{
    Task<TaskId> CreateAsync(IWorkflowTask request, CancellationToken cancellationToken);
    Task CancelActiveTasksAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task UpdateAsync(IWorkflowTask request, CancellationToken cancellationToken);
}