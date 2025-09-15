using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowService
{
    Task<WorkflowId> CreateAsync(IWorkflowContext workflowContext, CancellationToken cancellationToken);
    Task MarkAsInvalidAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task UpdateStateAsync<TContext>(TContext workflowContext, BaseState phase, CancellationToken cancellationToken) 
        where TContext : IWorkflowContext, new();
}