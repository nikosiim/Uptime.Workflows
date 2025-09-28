using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Application.Commands;

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
        
        string workflowBaseIdString = await db.WorkflowTemplates
            .Where(wt => wt.Id == request.WorkflowTemplateId.Value)
            .Select(wt => wt.WorkflowBaseId)
            .FirstAsync(ct);
         
        if (string.IsNullOrEmpty(workflowBaseIdString))
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow template with ID {request.WorkflowTemplateId.Value} not found.");
        
        var payload = new StartWorkflowPayload
        {
            ExecutorSid = request.ExecutorSid,
            DocumentId = request.DocumentId,
            WorkflowTemplateId = request.WorkflowTemplateId,
            Storage = request.Storage
        };

        return await factory.StartWorkflowAsync(workflowBaseIdString, payload, ct);
    }
}