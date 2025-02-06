using Uptime.Application.DTOs;
using Uptime.Shared.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowService
{
    Task<int> CreateWorkflowInstanceAsync(IWorkflowPayload payload);
    Task<WorkflowDto?> GetWorkflowInstanceAsync(int workflowId);
    Task UpdateWorkflowStateAsync<TData>(int workflowId, WorkflowStatus status, TData context);
}