using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface ITaskService
{
    Task CreateAsync(WorkflowId workflowId, IWorkflowActivityContext activityContext, CancellationToken cancellationToken);
    Task CancelActiveTasksAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task UpdateAsync(IWorkflowActivityContext context, CancellationToken cancellationToken);
}