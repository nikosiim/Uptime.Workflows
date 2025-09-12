using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Core.Services;

public class WorkflowService(IDbContextFactory<WorkflowDbContext> factory) : IWorkflowService
{
    public async Task<WorkflowId> CreateAsync(IWorkflowContext workflowContext, CancellationToken cancellationToken)
    {
        var instance = new Workflow
        {
            IsActive = true,
            Phase = BaseState.NotStarted.Value,
            StorageJson = workflowContext.Serialize(),
            StartDate = DateTime.UtcNow,
            DocumentId = workflowContext.GetDocumentId().Value,
            WorkflowTemplateId = workflowContext.GetWorkflowTemplateId().Value,
            InitiatedByPrincipalId = workflowContext.GetInitiatorId().Value
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

    public async Task UpdateStateAsync<TContext>(TContext workflowContext, BaseState phase, CancellationToken cancellationToken) 
        where TContext : IWorkflowContext, new()
    {
        int workflowId = workflowContext.GetWorkflowId().Value;

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        Workflow? instance = await db.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }

        var existingContext = BaseWorkflowContext.Deserialize<TContext>(instance.StorageJson);
        existingContext.Storage.MergeWith(workflowContext.Storage);

        instance.Phase = phase.Value;
        instance.StorageJson = JsonSerializer.Serialize(workflowContext);
        instance.Outcome = workflowContext.Outcome.Value;

        if (phase.IsFinal())
        {
            instance.EndDate = DateTime.UtcNow;
            instance.IsActive = false;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}