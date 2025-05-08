using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public interface IWorkflowService
{
    Task<WorkflowId> CreateAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Task MarkAsInvalidAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task UpdateStateAsync<TContext>(
        WorkflowId workflowId, 
        BaseState phase, 
        TContext context, 
        CancellationToken cancellationToken) 
        where TContext : IWorkflowContext, new();
}