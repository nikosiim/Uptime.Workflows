using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Common;

/// <summary>
/// Repository responsible for retrieving and materializing the workflow state data
/// from the persistence layer.
/// </summary>
/// <typeparam name="TContext">The type of the workflow context.</typeparam>
public class WorkflowStateRepository<TContext>(IWorkflowPersistenceService workflowService, ILogger<WorkflowStateRepository<TContext>> logger) 
    : IWorkflowStateRepository<TContext> where TContext : IWorkflowContext, new()
{
    /// <summary>
    /// Retrieves the persisted state of the workflow instance and materializes it into a <see cref="WorkflowStateData{TContext}"/> object.
    /// </summary>
    /// <param name="workflowId">The unique identifier of the workflow instance.</param>
    /// <returns>
    /// A <see cref="WorkflowStateData{TContext}"/> containing the current phase and deserialized workflow context,
    /// or <c>null</c> if the workflow instance cannot be found.
    /// </returns>
    public async Task<WorkflowStateData<TContext>?> GetWorkflowStateAsync(WorkflowId workflowId)
    {
        WorkflowDto? dto = await workflowService.GetWorkflowInstanceAsync(workflowId);
        if (dto == null)
        {
            logger.LogWarning("Workflow instance {WorkflowId} was not found.", workflowId);
            return null;
        }

        TContext context;
        if (string.IsNullOrWhiteSpace(dto.InstanceDataJson))
        {
            logger.LogWarning("Workflow instance {WorkflowId} has empty or missing instance data JSON. Using a new {TContext} instance.", workflowId, typeof(TContext).Name);
            context = new TContext();
        }
        else
        {
            try
            {
                context = JsonSerializer.Deserialize<TContext>(dto.InstanceDataJson) ?? new TContext();
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize workflow instance data for WorkflowId {WorkflowId}. Using a new {TContext} instance.", workflowId, typeof(TContext).Name);
                context = new TContext();
            }
        }

        return new WorkflowStateData<TContext>
        {
            Phase = dto.Phase,
            Context = context
        };
    }
}