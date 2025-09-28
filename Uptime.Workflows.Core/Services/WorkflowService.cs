using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public class WorkflowService(IDbContextFactory<WorkflowDbContext> factory, IPrincipalResolver principalResolver) : IWorkflowService
{
    public async Task<WorkflowId> CreateAsync(IWorkflowContext context, CancellationToken ct)
    {
        PrincipalSid initiatorSid = context.GetInitiatorSid();
        Principal initiator = await principalResolver.ResolveBySidAsync(initiatorSid, ct);

        var instance = new Workflow
        {
            IsActive = true,
            Phase = BaseState.NotStarted.Value,
            StorageJson = context.Serialize(),
            StartDate = DateTime.UtcNow,
            DocumentId = context.GetDocumentId().Value,
            WorkflowTemplateId = context.GetWorkflowTemplateId().Value,
            InitiatedByPrincipalId = initiator.Id.Value
        };

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        db.Workflows.Add(instance);
        await db.SaveChangesAsync(ct);

        return (WorkflowId)instance.Id;
    }

    public async Task MarkAsInvalidAsync(WorkflowId workflowId, CancellationToken ct)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        Workflow? instance = await db.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, ct);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }

        instance.IsActive = false;
        instance.Outcome = WorkflowOutcome.Invalid.Value;
        instance.Phase = BaseState.Invalid.Value;
        instance.EndDate = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateStateAsync<TContext>(TContext workflowContext, BaseState phase, CancellationToken ct) 
        where TContext : IWorkflowContext, new()
    {
        int workflowId = workflowContext.GetWorkflowId().Value;

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        Workflow? instance = await db.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId, ct);
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

        await db.SaveChangesAsync(ct);
    }
}