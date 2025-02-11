using Uptime.Application.DTOs;
using Uptime.Domain.Common;

namespace Uptime.Application.Interfaces;

public interface IWorkflowStateRepository<TContext>
    where TContext : IWorkflowContext, new()
{
    Task<WorkflowStateData<TContext>?> GetWorkflowStateAsync(WorkflowId workflowId);
}