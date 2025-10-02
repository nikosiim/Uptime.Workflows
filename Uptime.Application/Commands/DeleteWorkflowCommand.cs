using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Application.Commands;

public record DeleteWorkflowCommand : IRequest<Result<Unit>>, IRequiresPrincipal
{
    public required WorkflowId WorkflowId { get; init; }
    public required PrincipalSid ExecutorSid { get; init; }
}

public class DeleteWorkflowCommandHandler(WorkflowDbContext db, IPrincipalResolver principalResolver, ILogger<DeleteWorkflowCommandHandler> logger)
    : IRequestHandler<DeleteWorkflowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteWorkflowCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        Workflow? workflow = await db.Workflows
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, ct);

        if (workflow is null)
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow {request.WorkflowId} not found.");

        Principal executor = await principalResolver.ResolveBySidAsync(request.ExecutorSid, ct);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // Soft delete + audit
        workflow.IsDeleted = true;
        workflow.DeletedAtUtc = now;
        workflow.DeletedByPrincipalId = executor.Id.Value;
        workflow.UpdatedAtUtc = now;
        workflow.UpdatedByPrincipalId = executor.Id.Value;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Workflow [{WorkflowId}] deleted by {ExecutorSid}.", request.WorkflowId, request.ExecutorSid.Value);

        return Result<Unit>.Success(new Unit());
    }
}