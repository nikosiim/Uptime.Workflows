using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record  StartWorkflowCommand : IRequest<Result<Unit>>
{
    public required string CallerSid { get; init; }
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
        
        string workflowBaseIdString = await db.WorkflowTemplates
            .Where(wt => wt.Id == request.WorkflowTemplateId.Value)
            .Select(wt => wt.WorkflowBaseId)
            .FirstAsync(ct);
         
        if (string.IsNullOrEmpty(workflowBaseIdString))
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow template with ID {request.WorkflowTemplateId.Value} not found.");
        
        var payload = new StartWorkflowPayload
        {
            PrincipalSid = request.CallerSid,
            WorkflowBaseId = new Guid(workflowBaseIdString),
            DocumentId = request.DocumentId,
            WorkflowTemplateId = request.WorkflowTemplateId,
            Storage = request.Storage
        };

        return await factory.StartWorkflowAsync(payload, ct);
    }
}