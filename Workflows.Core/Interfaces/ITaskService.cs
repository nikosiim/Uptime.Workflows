using Workflows.Core.Common;

namespace Workflows.Core.Interfaces;

public interface ITaskService
{
    Task CreateAsync(WorkflowId workflowId, PrincipalId assignedBy, PrincipalId assignedTo, IWorkflowActivityContext context, CancellationToken ct);
    Task CancelActiveTasksAsync(WorkflowId workflowId, CancellationToken ct);
    Task UpdateAsync(IWorkflowActivityContext context, CancellationToken ct);
}