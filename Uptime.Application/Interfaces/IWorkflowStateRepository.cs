using Uptime.Application.DTOs;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowStateRepository<TContext>
    where TContext : IWorkflowContext, new()
{
    Task<WorkflowId> CreateWorkflowStateAsync(IWorkflowPayload payload);

    /// <summary>
    /// Retrieves the workflow state for the specified workflow ID.
    /// </summary>
    /// <param name="workflowId">The unique identifier of the workflow.</param>
    /// <returns>
    /// A <see cref="WorkflowStateData{TContext}"/> object containing the current phase and context,
    /// or <c>null</c> if the workflow instance is not found.
    /// </returns>
    Task<WorkflowStateData<TContext>?> GetWorkflowStateAsync(WorkflowId workflowId);

    /// <summary>
    /// Persists the workflow state changes for the specified workflow ID.
    /// </summary>
    /// <param name="workflowId">The unique identifier of the workflow.</param>
    /// <param name="phase">The new workflow phase.</param>
    /// <param name="context">The updated workflow context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveWorkflowStateAsync(WorkflowId workflowId, WorkflowPhase phase, TContext context);
}