using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public class WorkflowService(IDbContextFactory<WorkflowDbContext> factory) : IWorkflowService
{
    public async Task<WorkflowId> CreateAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        var instance = new Workflow
        {
            IsActive = true,
            Phase = BaseState.NotStarted.Value,
            StorageJson = null,
            Originator = payload.Originator,
            StartDate = DateTime.UtcNow,
            DocumentId = payload.DocumentId.Value,
            WorkflowTemplateId = payload.WorkflowTemplateId.Value
        };

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        db.Workflows.Add(instance);
        await db.SaveChangesAsync(cancellationToken);

        return (WorkflowId)instance.Id;
    }

    public async Task MarkAsInvalidAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        Workflow? instance = await db.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }

        instance.IsActive = false;
        instance.Outcome = WorkflowOutcome.Invalid.Value;
        instance.Phase = BaseState.Invalid.Value;
        instance.EndDate = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStateAsync<TContext>(WorkflowId workflowId, BaseState phase, TContext context, CancellationToken cancellationToken)
        where TContext : IWorkflowContext, new()
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        Workflow? instance = await db.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }

        var existingContext = WorkflowContextHelper.Deserialize<TContext>(instance.StorageJson);
        WorkflowContextHelper.MergeContext(existingContext, context);

        instance.Phase = phase.Value;
        instance.StorageJson = JsonSerializer.Serialize(context);
        instance.Outcome = context.Outcome.Value;

        if (phase.IsFinal())
        {
            instance.EndDate = DateTime.UtcNow;
            instance.IsActive = false;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}