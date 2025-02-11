using Uptime.Application.DTOs;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowPersistenceService
{
    Task<WorkflowId> CreateWorkflowInstanceAsync(IWorkflowPayload payload);
    Task<WorkflowDto?> GetWorkflowInstanceAsync(WorkflowId workflowId);
    Task UpdateWorkflowStateAsync<TContext>(WorkflowId workflowId, WorkflowPhase phase, TContext context) where TContext : IWorkflowContext, new();
}