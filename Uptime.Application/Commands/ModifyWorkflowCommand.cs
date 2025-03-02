using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Commands;

public record ModifyWorkflowCommand(ModificationContext ModificationContext) : IRequest<string>;

public class ModifyWorkflowCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory, ILogger<ModifyWorkflowCommand> logger)
    : IRequestHandler<ModifyWorkflowCommand, string>
{
    public async Task<string> Handle(ModifyWorkflowCommand request, CancellationToken cancellationToken)
    {
        // TODO: pass WorkflowBaseId and skip workflow search
        Workflow? workflowInstance = await dbContext.Workflows.AsNoTracking()
            .Include(w => w.WorkflowTemplate)
            .Where(x => x.Id == request.ModificationContext.WorkflowId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (workflowInstance == null)
            throw new InvalidOperationException($"Workflow with ID {request.ModificationContext.WorkflowId} not found.");
        
        var baseId = new Guid(workflowInstance.WorkflowTemplate.WorkflowBaseId);
        
        IWorkflowMachine? stateMachine = workflowFactory.TryGetStateMachine(baseId);
        if (stateMachine is not IReplicatorActivityWorkflowMachine machine)
        {
            logger.LogWarning("The workflow with ID {WorkflowBaseId} does not support workflow modification.", baseId);
            return BaseState.Invalid.Value;
        }
        
        bool isRehydrated = await machine.RehydrateAsync((WorkflowId)workflowInstance.Id, cancellationToken);
        if (!isRehydrated)
        {
            return BaseState.Invalid.Value;
        }
        
        return await machine.ModifyWorkflow(request.ModificationContext, cancellationToken);
    }
}