using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.DTOs;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public class WorkflowStateRepository<TContext>(IWorkflowDbContext dbContext, ILogger<WorkflowStateRepository<TContext>> logger)
    : IWorkflowStateRepository<TContext> where TContext : IWorkflowContext, new()
{
    public async Task<WorkflowId> CreateWorkflowStateAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        var instance = new Workflow
        {
            Phase = WorkflowPhase.NotStarted,
            StorageJson = null,
            Originator = payload.Originator,
            StartDate = DateTime.UtcNow,
            DocumentId = payload.DocumentId.Value,
            WorkflowTemplateId = payload.WorkflowTemplateId.Value
        };

        dbContext.Workflows.Add(instance);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (WorkflowId)instance.Id;
    }

    public async Task<WorkflowStateData<TContext>?> GetWorkflowStateAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        Workflow? instance = await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, cancellationToken);
        if (instance == null)
        {
            logger.LogWarning("Workflow instance {WorkflowId} not found.", workflowId);
            return null;
        }
        
        TContext context;

        if (string.IsNullOrWhiteSpace(instance.StorageJson))
        {
            logger.LogWarning("Workflow instance {WorkflowId} has empty or missing instance data JSON. Using a new {TContext} instance.", workflowId, typeof(TContext).Name);
            context = new TContext();
        }
        else
        {
            try
            {
                context = JsonSerializer.Deserialize<TContext>(instance.StorageJson) ?? new TContext();
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize workflow instance data for WorkflowId {WorkflowId}. Using a new {TContext} instance.", workflowId, typeof(TContext).Name);
                context = new TContext();
            }
        }

        return new WorkflowStateData<TContext>
        {
            Phase = instance.Phase,
            Context = context
        };
    }

    public async Task SaveWorkflowStateAsync(WorkflowId workflowId, WorkflowPhase phase, TContext context, CancellationToken cancellationToken)
    {
        Workflow? instance = await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, cancellationToken: cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }

        // Merge the existing context's storage with the updated context's storage.
        TContext existingContext = new();
        existingContext.Deserialize(instance.StorageJson ?? "{}");
        existingContext.Storage.MergeWith(context.Storage);

        instance.Phase = phase;
        instance.StorageJson = JsonSerializer.Serialize(context);

        if (phase.IsFinal())
        {
            instance.EndDate = DateTime.Now.ToUniversalTime();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}