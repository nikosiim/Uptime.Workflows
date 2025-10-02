using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Application.Commands;

public record ModifyWorkflowCommand : IRequest<Result<Unit>>, IRequiresPrincipal
{
    public required PrincipalSid ExecutorSid { get; init; }
    public required WorkflowId WorkflowId { get; init; }
    public string? InputContext { get; init; }
};

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

        Result<Unit> rehydrationResult = await machine.Rehydrate(workflow.StorageJson!, workflow.Phase, ct);
        if (!rehydrationResult.Succeeded)
            return rehydrationResult;

        var payload = new ModificationPayload
        {
            ExecutorSid = request.ExecutorSid,
            ModificationContext = request.InputContext
        };

        return await machine.ModifyAsync(payload, ct);
    }
}