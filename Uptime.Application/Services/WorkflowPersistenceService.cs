using MediatR;
using System.Text.Json;
using Uptime.Application.Commands;
using Uptime.Application.Common;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Queries;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Services;

public sealed class WorkflowPersistenceService(IMediator mediator) : IWorkflowPersistenceService
{
    public async Task<WorkflowId> CreateWorkflowInstanceAsync(IWorkflowPayload payload)
    {
        var createCmd = new CreateWorkflowInstanceCommand
        {
            Originator = payload.Originator,
            DocumentId = payload.DocumentId,
            WorkflowTemplateId = payload.WorkflowTemplateId
        };

        return await mediator.Send(createCmd);
    }

    public async Task<WorkflowDto?> GetWorkflowInstanceAsync(WorkflowId workflowId)
    {
        return await mediator.Send(new GetWorkflowQuery(workflowId));
    }

    public async Task UpdateWorkflowStateAsync<TContext>(WorkflowId workflowId, WorkflowPhase phase, TContext context) 
        where TContext : IWorkflowContext, new()
    {
        WorkflowDto? existingWorkflow = await GetWorkflowInstanceAsync(workflowId);
        if (existingWorkflow == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }
        
        TContext existingContext = new();
        existingContext.Deserialize(existingWorkflow.InstanceDataJson ?? "{}");
        existingContext.Storage.MergeWith(context.Storage);

        var cmd = new UpdateWorkflowStateCommand
        {
            WorkflowId = workflowId,
            Phase = phase,
            StorageJson = JsonSerializer.Serialize(context)
        };

        if (phase == WorkflowPhase.Completed)
        {
            cmd.EndDate = DateTime.Now.ToUniversalTime();
        }
        
        await mediator.Send(cmd);
    }
}