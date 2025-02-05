using MediatR;
using System.Text.Json;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Models.Common;
using Uptime.Application.Queries;
using Uptime.Shared.Enums;

namespace Uptime.Application.Services;

public interface IWorkflowService
{
    Task<int> CreateWorkflowInstanceAsync(WorkflowPayload payload);
    Task<WorkflowDto?> GetWorkflowInstanceAsync(int workflowId);
    Task UpdateWorkflowStateAsync<TData>(int workflowId, WorkflowStatus status, TData context);
}

public sealed class WorkflowService(IMediator mediator) : IWorkflowService
{
    public async Task<int> CreateWorkflowInstanceAsync(WorkflowPayload payload)
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