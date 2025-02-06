using MediatR;
using Uptime.Application.Common;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Queries;
using Uptime.Application.Workflows.Approval;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record StartWorkflowCommand : IRequest<WorkflowStatus>
{
    public required string Originator { get; init; }
    public required int DocumentId { get; init; }
    public required int WorkflowTemplateId { get; init; }
    public Dictionary<string, object?> Data { get; init; } = new();
}

public class StartWorkflowCommandHandler(IWorkflowService workflowService, ITaskService taskService, IMediator mediator)
    : IRequestHandler<StartWorkflowCommand, WorkflowStatus>
{
    public async Task<WorkflowStatus> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        WorkflowTemplateDto? workflowTemplate = await mediator.Send(new GetWorkflowTemplateQuery(request.WorkflowTemplateId), cancellationToken);
        if (workflowTemplate == null)
        {
            return WorkflowStatus.Invalid;
        }

        if (workflowTemplate.WorkflowBaseId == "16778969-6d4c-4367-9106-1b0ae4a4594f")
        {
            // TODO: use mapping if desired
            var payload = new StartWorkflowPayload
            {
                Originator = request.Originator,
                DocumentId = request.DocumentId,
                WorkflowTemplateId = request.WorkflowTemplateId,
                Data = request.Data
            };

            var workflow = new ApprovalWorkflow(workflowService, taskService);
            return await workflow.StartAsync(payload);
        }
        
        if (workflowTemplate.WorkflowBaseId == "BA0E8F92-5030-4E24-8BC8-A2A9DF622133")
        {
            // Signing workflow
        }

        return WorkflowStatus.Invalid;
    }
}