using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Queries;

public record GetModificationDataQuery(WorkflowId WorkflowId) : IRequest<ModificationContext?>;

public class GetModificationDataQueryHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory, ILogger<GetModificationDataQuery> logger) 
    : IRequestHandler<GetModificationDataQuery, ModificationContext?>
{
    public async Task<ModificationContext?> Handle(GetModificationDataQuery request, CancellationToken cancellationToken)
    {
        Workflow? workflow = await dbContext.Workflows.AsNoTracking()
            .Include(w => w.WorkflowTemplate)
            .Where(x => x.Id == request.WorkflowId.Value)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (workflow == null)
            throw new InvalidOperationException($"Workflow with ID {request.WorkflowId} not found.");

        var baseId = new Guid(workflow.WorkflowTemplate.WorkflowBaseId);
        
        IWorkflowMachine? stateMachine = workflowFactory.TryGetStateMachine(baseId);
        if (stateMachine is not IReplicatorActivityWorkflowMachine machine)
        {
            logger.LogWarning("Workflow {Id} does not implement IReplicatorWorkflowContext", workflow.Id);
            return null;
        }

        if (!await machine.RehydrateAsync(workflow, cancellationToken))
        {
            logger.LogWarning("Workflow {Id} state-machine reHydration failed.", workflow.Id);
            return null;
        }

        return machine.GetModificationContext(workflow.Phase);
    }
}