using MediatR;
using Uptime.Application.Common;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Queries;
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

public class StartWorkflowCommandHandler(IWorkflowFactory workflowFactory, IMediator mediator)
    : IRequestHandler<StartWorkflowCommand, WorkflowPhase>
{
    public async Task<WorkflowPhase> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        WorkflowTemplateDto? workflowTemplate = await mediator.Send(new GetWorkflowTemplateQuery(request.WorkflowTemplateId), cancellationToken);
        if (workflowTemplate == null)
        {
            return WorkflowPhase.Invalid;
        }

        var payload = new StartWorkflowPayload
        {
            Originator = request.Originator,
            DocumentId = request.DocumentId,
            WorkflowTemplateId = request.WorkflowTemplateId,
            Storage = request.Storage
        };

        return await workflowFactory.StartWorkflowAsync(Guid.Parse(workflowTemplate.WorkflowBaseId), payload);
    }
}