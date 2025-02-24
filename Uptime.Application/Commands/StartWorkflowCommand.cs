using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Commands;

public record StartWorkflowCommand : IRequest<string>
{
    public required string Originator { get; init; }
    public required DocumentId DocumentId { get; init; }
    public required WorkflowTemplateId WorkflowTemplateId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class StartWorkflowCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory)
    : IRequestHandler<StartWorkflowCommand, string>
{
    public async Task<string> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        string workflowBaseIdString = await dbContext.WorkflowTemplates
            .Where(wt => wt.Id == request.WorkflowTemplateId.Value)
            .Select(wt => wt.WorkflowBaseId)
            .FirstAsync(cancellationToken);

        if (!Guid.TryParse(workflowBaseIdString, out Guid workflowBaseId))
        {
            return BaseState.Invalid.Value;
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