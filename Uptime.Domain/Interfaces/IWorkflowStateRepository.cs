using Uptime.Domain.Common;
using Uptime.Domain.DTOs;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowStateRepository<TContext>
    where TContext : IWorkflowContext, new()
{
    Task<WorkflowId> CreateWorkflowStateAsync(IWorkflowPayload payload, CancellationToken cancellationToken);

    Task<WorkflowStateData<TContext>?> GetWorkflowStateAsync(WorkflowId workflowId, CancellationToken cancellationToken);

    Task SaveWorkflowStateAsync(WorkflowId workflowId, WorkflowPhase phase, TContext context, CancellationToken cancellationToken);
}