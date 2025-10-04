using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Workflows.Core.Common;
using Workflows.Core.Data;
using Workflows.Core.Extensions;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace Workflows.Core.Services;

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
            InitiatedById = initiator.Id.Value,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedByPrincipalId = initiator.Id.Value,
            IsDeleted = false
        };

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        db.Workflows.Add(instance);
        await db.SaveChangesAsync(ct);

        return (WorkflowId)instance.Id;
    }

    public async Task MarkAsInvalidAsync(WorkflowId workflowId, PrincipalSid actorSid, CancellationToken ct)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        Workflow? instance = await db.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, ct);
        if (instance == null)
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");

        Principal actor = await principalResolver.ResolveBySidAsync(actorSid, ct);

        instance.IsActive = false;
        instance.Outcome = WorkflowOutcome.Invalid.Value;
        instance.Phase = BaseState.Invalid.Value;
        instance.EndDate = DateTime.UtcNow;
        instance.UpdatedAtUtc = DateTimeOffset.UtcNow;
        instance.UpdatedByPrincipalId = actor.Id.Value;

        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateStateAsync<TContext>(TContext workflowContext, BaseState phase, PrincipalSid actorSid, CancellationToken ct) 
        where TContext : IWorkflowContext, new()
    {
        int workflowId = workflowContext.GetWorkflowId().Value;

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        Workflow? instance = await db.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId, ct);
        if (instance == null)
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");

        Principal actor = await principalResolver.ResolveBySidAsync(actorSid, ct);

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

        instance.UpdatedAtUtc = DateTimeOffset.UtcNow;
        instance.UpdatedByPrincipalId = actor.Id.Value;

        await db.SaveChangesAsync(ct);
    }
}