using MediatR;
using System.Text.Json;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Queries;
using Uptime.Shared.Enums;

namespace Uptime.Application.Services;

public sealed class WorkflowService(IMediator mediator) : IWorkflowService
{
    public async Task<int> CreateWorkflowInstanceAsync(IWorkflowPayload payload)
    {
        var createCmd = new CreateWorkflowInstanceCommand
        {
            Originator = payload.Originator,
            DocumentId = payload.DocumentId,
            WorkflowTemplateId = payload.WorkflowTemplateId
        };

        return await mediator.Send(createCmd);
    }

    public async Task<WorkflowDto?> GetWorkflowInstanceAsync(int workflowId)
    {
        return await mediator.Send(new GetWorkflowQuery(workflowId));
    }

    public async Task UpdateWorkflowStateAsync<TData>(int workflowId, WorkflowStatus status, TData context)
    {
        await mediator.Send(new UpdateWorkflowStateCommand
        {
            WorkflowId = workflowId,
            Status = status,
            InstanceDataJson = JsonSerializer.Serialize(context)
        });
    }
}