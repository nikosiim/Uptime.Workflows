using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record CancelWorkflowCommand(WorkflowId WorkflowId, string Executor, string Comment) : IRequest<Result<Unit>>;

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
            return Result<Unit>.Failure(ErrorCode.NotFound);
        
        IWorkflowMachine? machine = factory.TryGetStateMachine(workflow.WorkflowTemplate.WorkflowBaseId);
        if (machine == null)
        {
            log.LogError("State-machine not found {WorkflowBaseId}", workflow.WorkflowTemplate.WorkflowBaseId);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }
        
        Result<Unit> rehydrationResult = machine.Rehydrate(workflow, ct);
        if (!rehydrationResult.Succeeded)
            return rehydrationResult;

        return await machine.CancelAsync(request.Executor, request.Comment, ct);
    }
}