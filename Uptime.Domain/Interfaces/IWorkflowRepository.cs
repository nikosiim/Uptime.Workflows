using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowRepository
{
    #region Workflows

    Task<WorkflowId> CreateWorkflowInstanceAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Task MarkWorkflowAsInvalidAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task<Workflow?> GetWorkflowInstanceAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task SaveWorkflowStateAsync<TContext>(WorkflowId workflowId, WorkflowPhase phase, TContext context, CancellationToken cancellationToken) where TContext : IWorkflowContext, new();
    
    #endregion

    #region WorkflowTasks

    Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask task, CancellationToken cancellationToken);
    Task CancelAllActiveTasksAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task SaveWorkflowTaskAsync(IWorkflowTask task, CancellationToken cancellationToken);

    #endregion

    #region WorkflowHistory

    Task AddWorkflowHistoryAsync(
        WorkflowId workflowId,
        WorkflowHistoryEventType eventType,
        string? user,
        string? outcome,
        string? description,
        CancellationToken cancellationToken);

    #endregion
}