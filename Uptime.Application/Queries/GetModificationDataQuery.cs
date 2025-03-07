using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Queries;

public record GetModificationDataQuery(WorkflowId WorkflowId) : IRequest<Result<string?>>;

public sealed class GetModificationDataQueryHandler(
    IWorkflowDbContext dbContext,
    IWorkflowFactory workflowFactory,
    ILogger<GetModificationDataQueryHandler> logger)
    : IRequestHandler<GetModificationDataQuery, Result<string?>>
{
    public async Task<Result<string?>> Handle(GetModificationDataQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<string?>.Cancelled();

        Workflow? workflow = await dbContext.Workflows.AsNoTracking()
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, cancellationToken);

        if (workflow == null)
        {
            return Result<string?>.Failure($"Workflow with ID {request.WorkflowId} not found.");
        }

        var baseId = new Guid(workflow.WorkflowTemplate.WorkflowBaseId);

        IWorkflowMachine? stateMachine = workflowFactory.TryGetStateMachine(baseId);
        if (stateMachine == null)
        {
            logger.LogWarning("Failed to detect-state machine for workflow {Id}", workflow.Id);
            return Result<string?>.Failure("Invalid workflow machine type.");
        }

        if (!await stateMachine.RehydrateAsync(workflow, cancellationToken))
        {
            logger.LogWarning("State-machine reHydration failed for workflow {Id}.", workflow.Id);
            return Result<string?>.Failure("Workflow state-machine reHydration failed.");
        }

        return stateMachine.GetModificationContext();
    }
}
