using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface ITaskService
{
    Task<TaskId> CreateAsync(WorkflowId workflowId, IWorkflowTaskContext taskContext, CancellationToken cancellationToken);
    Task CancelActiveTasksAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task UpdateAsync(IWorkflowTaskContext taskContext, CancellationToken cancellationToken);
}