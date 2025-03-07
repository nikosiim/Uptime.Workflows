using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;
using Unit = Uptime.Domain.Common.Unit;

namespace Uptime.Application.Queries;

public record GetModificationContextQuery(WorkflowId WorkflowId) : IRequest<Result<string>>;

public sealed class GetModificationContextQueryHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory)
    : IRequestHandler<GetModificationContextQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetModificationContextQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<string>.Cancelled();

        Workflow? workflow = await dbContext.Workflows.AsNoTracking()
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, cancellationToken);

        if (workflow == null)
            return Result<string>.Failure($"Workflow with ID {request.WorkflowId.Value} not found.");
        
        IWorkflowMachine? stateMachine = workflowFactory.TryGetStateMachine(workflow.WorkflowTemplate.WorkflowBaseId);
        if (stateMachine == null) 
            return Result<string>.Failure("Invalid workflow machine type.");

        Result<Unit> reHydrationResult = stateMachine.RehydrateAsync(workflow, cancellationToken);

        return !reHydrationResult.Succeeded
            ? Result<string>.Failure("Workflow state-machine reHydration failed.")
            : stateMachine.GetModificationContext();
    }
}