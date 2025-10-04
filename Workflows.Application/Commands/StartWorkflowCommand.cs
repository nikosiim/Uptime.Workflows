using Microsoft.EntityFrameworkCore;
using Workflows.Application.Messaging;
using Workflows.Core.Common;
using Workflows.Core.Data;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace Workflows.Application.Commands;

public record StartWorkflowCommand : IRequiresPrincipal, IRequest<Result<Unit>>
{
    public required PrincipalSid ExecutorSid { get; init; }
    public required DocumentId DocumentId { get; init; }
    public required WorkflowTemplateId WorkflowTemplateId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class StartWorkflowCommandHandler(WorkflowDbContext db, IWorkflowFactory factory)
    : IRequestHandler<StartWorkflowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(StartWorkflowCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        var tpl = await db.WorkflowTemplates
            .AsNoTracking()
            .Where(wt => wt.Id == request.WorkflowTemplateId.Value)
            .Select(wt => new { wt.WorkflowBaseId, wt.SiteUrl })
            .FirstOrDefaultAsync(ct);

        if (tpl is null || string.IsNullOrEmpty(tpl.WorkflowBaseId))
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow template with ID {request.WorkflowTemplateId.Value} not found.");

        var payload = new StartWorkflowPayload
        {
            ExecutorSid = request.ExecutorSid,
            SourceSiteUrl = tpl.SiteUrl,
            DocumentId = request.DocumentId,
            WorkflowTemplateId = request.WorkflowTemplateId,
            Storage = request.Storage
        };

        return await factory.StartWorkflowAsync(tpl.WorkflowBaseId, payload, ct);
    }
}