using Workflows.Core.Common;

namespace Workflows.Core.Interfaces;

public interface IWorkflowService
{
    Task<WorkflowId> CreateAsync(IWorkflowContext context, CancellationToken ct);
    Task MarkAsInvalidAsync(WorkflowId workflowId, PrincipalSid actorSid, CancellationToken ct);
    Task UpdateStateAsync<TContext>(TContext workflowContext, BaseState phase, PrincipalSid actorSid, CancellationToken ct) 
        where TContext : IWorkflowContext, new();
}