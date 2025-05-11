using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record ModifyWorkflowCommand(WorkflowId WorkflowId, ModificationPayload Payload) : IRequest<Result<Unit>>;

public class ModifyWorkflowCommandHandler(WorkflowDbContext db, IWorkflowFactory factory, ILogger<ModifyWorkflowCommand> log)
    : IRequestHandler<ModifyWorkflowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ModifyWorkflowCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();
        
        Workflow? workflow = await db.Workflows
            .Include(w => w.WorkflowTemplate)
            .Where(x => x.Id == request.WorkflowId.Value)
            .FirstOrDefaultAsync(cancellationToken: ct);
        
        if (workflow is null)
            return Result<Unit>.Failure(ErrorCode.NotFound);

        IWorkflowMachine? machine = factory.TryGetStateMachine(workflow.WorkflowTemplate.WorkflowBaseId);
        if (machine is null)
        {
            log.LogError("State-machine not found {WorkflowBaseId}", workflow.WorkflowTemplate.WorkflowBaseId);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }

        Result<Unit> rehydrationResult = machine.Rehydrate(workflow, ct);
        if (!rehydrationResult.Succeeded)
            return rehydrationResult;

        return await machine.ModifyAsync(request.Payload, ct);
    }
}