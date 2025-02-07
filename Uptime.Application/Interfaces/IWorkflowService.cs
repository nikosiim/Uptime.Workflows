using Uptime.Application.DTOs;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowService
{
    Task<WorkflowId> CreateWorkflowInstanceAsync(IWorkflowPayload payload);
    Task<WorkflowDto?> GetWorkflowInstanceAsync(WorkflowId workflowId);
    Task UpdateWorkflowStateAsync<TData>(WorkflowId workflowId, WorkflowStatus status, TData context);
}