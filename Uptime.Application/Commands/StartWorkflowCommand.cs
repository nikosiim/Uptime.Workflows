using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
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

public class StartWorkflowCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory)
    : IRequestHandler<StartWorkflowCommand, WorkflowPhase>
{
    public async Task<WorkflowPhase> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        string workflowBaseIdString = await dbContext.WorkflowTemplates
            .Where(wt => wt.Id == request.WorkflowTemplateId.Value)
            .Select(wt => wt.WorkflowBaseId)
            .FirstAsync(cancellationToken);

        if (!Guid.TryParse(workflowBaseIdString, out Guid workflowBaseId))
        {
            return WorkflowPhase.Invalid;
        }

        var payload = new StartWorkflowPayload
        {
            WorkflowBaseId = workflowBaseId,
            Originator = request.Originator,
            DocumentId = request.DocumentId,
            WorkflowTemplateId = request.WorkflowTemplateId,
            Storage = request.Storage
        };

        return await workflowFactory.StartWorkflowAsync(payload, cancellationToken);
    }
}