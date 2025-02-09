using MediatR;
using Uptime.Application.Common;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Application.Workflows.Approval;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Commands;

public record StartWorkflowCommand : IRequest<WorkflowPhase>
{
    public required string Originator { get; init; }
    public required DocumentId DocumentId { get; init; }
    public required WorkflowTemplateId WorkflowTemplateId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class StartWorkflowCommandHandler(ApprovalWorkflow approvalWorkflow, IMediator mediator)
    : IRequestHandler<StartWorkflowCommand, WorkflowPhase>
{
    public async Task<WorkflowPhase> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        WorkflowTemplateDto? workflowTemplate = await mediator.Send(new GetWorkflowTemplateQuery(request.WorkflowTemplateId), cancellationToken);
        if (workflowTemplate == null)
        {
            return WorkflowPhase.Invalid;
        }

        if (workflowTemplate.WorkflowBaseId == "16778969-6d4c-4367-9106-1b0ae4a4594f")
        {
            var payload = new StartWorkflowPayload
            {
                Originator = request.Originator,
                DocumentId = request.DocumentId,
                WorkflowTemplateId = request.WorkflowTemplateId,
                Storage = request.Storage
            };
            
            return await approvalWorkflow.StartAsync(payload);
        }
        
        if (workflowTemplate.WorkflowBaseId == "BA0E8F92-5030-4E24-8BC8-A2A9DF622133")
        {
            // Signing workflow
        }

        return WorkflowPhase.Invalid;
    }
}