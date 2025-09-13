using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Queries;

public record GetModificationContextQuery(WorkflowId WorkflowId) : IRequest<Result<string>>;

public sealed class GetModificationContextQueryHandler(WorkflowDbContext db, IWorkflowFactory factory, ILogger<GetModificationContextQuery> log)
    : IRequestHandler<GetModificationContextQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetModificationContextQuery request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<string>.Cancelled();

        Workflow? workflow = await db.Workflows.AsNoTracking()
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, ct);

        if (workflow is null)
            return Result<string>.Failure(ErrorCode.NotFound);

        IWorkflowMachine? machine = factory.TryGetStateMachine(workflow.WorkflowTemplate.WorkflowBaseId);
        if (machine is null)
        {
            log.LogError("State-machine not found {WorkflowBaseId}", workflow.WorkflowTemplate.WorkflowBaseId);
            return Result<string>.Failure(ErrorCode.Unexpected);
        }

        Result<Unit> rehydrationResult = machine.Rehydrate(workflow.StorageJson!, workflow.Phase, ct);
        return !rehydrationResult.Succeeded 
            ? Result<string>.Failure(rehydrationResult.Code!.Value, rehydrationResult.Details) 
            : machine.GetModificationContext();
    }
}