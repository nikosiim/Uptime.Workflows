using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowRepository
{
    #region Workflows

    Task<WorkflowId> CreateWorkflowInstanceAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Task MarkWorkflowAsInvalidAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task SaveWorkflowStateAsync<TContext>(WorkflowId workflowId, BaseState phase, TContext context, CancellationToken cancellationToken) where TContext : IWorkflowContext, new();
    
    #endregion

    #region WorkflowTasks

    Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask request, CancellationToken cancellationToken);
    Task CancelAllActiveTasksAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task SaveWorkflowTaskAsync(IWorkflowTask request, CancellationToken cancellationToken);

    #endregion

    #region WorkflowHistory

    Task AddWorkflowHistoryAsync(
        WorkflowId workflowId,
        WorkflowEventType eventType,
        string? author,
        string? description,
        string? comment = null,
        CancellationToken cancellationToken = default);

    #endregion
}