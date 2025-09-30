using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Application.Commands;

public record CancelWorkflowCommand : IRequest<Result<Unit>>, IRequiresPrincipal
{
    public required PrincipalSid ExecutorSid { get; init; }
    public required WorkflowId WorkflowId { get; init; }
    public required string Comment { get; init; }
};

public class CancelWorkflowCommandHandler(WorkflowDbContext db, IWorkflowFactory factory, ILogger<CancelWorkflowCommand> log)
    : IRequestHandler<CancelWorkflowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CancelWorkflowCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        Workflow? workflow = await db.Workflows
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, ct);
        
        if (workflow == null)
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow with ID {request.WorkflowId.Value} not found.");
        
        IWorkflowMachine? machine = factory.TryGetStateMachine(workflow.WorkflowTemplate.WorkflowBaseId);
        if (machine == null)
        {
            log.LogError("State-machine not found {WorkflowBaseId}", workflow.WorkflowTemplate.WorkflowBaseId);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }
        
        Result<Unit> rehydrationResult = await machine.Rehydrate(workflow.StorageJson!, workflow.Phase, ct);
        if (!rehydrationResult.Succeeded)
            return rehydrationResult;

        var payload = new CancelWorkflowPayload
        {
            ExecutorSid = request.ExecutorSid,
            Comment = request.Comment
        };
        
        return await machine.CancelAsync(payload, ct);
    }
}