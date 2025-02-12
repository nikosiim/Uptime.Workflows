using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;
using Uptime.Shared.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowRepository
{
    Task<WorkflowId> CreateWorkflowInstanceAsync(IWorkflowPayload payload, CancellationToken cancellationToken);

    Task<Workflow?> GetWorkflowInstanceAsync(WorkflowId workflowId, CancellationToken cancellationToken);

    Task SaveWorkflowStateAsync<TContext>(WorkflowId workflowId, WorkflowPhase phase, TContext context, CancellationToken cancellationToken) where TContext : IWorkflowContext, new();

    Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask task, CancellationToken cancellationToken);

    Task SaveWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus status, CancellationToken cancellationToken);
}