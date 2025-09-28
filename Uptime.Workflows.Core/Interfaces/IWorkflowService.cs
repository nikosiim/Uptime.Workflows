using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowService
{
    Task<WorkflowId> CreateAsync(IWorkflowContext context, CancellationToken ct);
    Task MarkAsInvalidAsync(WorkflowId workflowId, CancellationToken ct);
    Task UpdateStateAsync<TContext>(TContext workflowContext, BaseState phase, CancellationToken ct) 
        where TContext : IWorkflowContext, new();
}