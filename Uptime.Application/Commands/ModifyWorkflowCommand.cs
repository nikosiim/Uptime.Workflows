using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;
using Unit = Uptime.Domain.Common.Unit;

namespace Uptime.Application.Commands;

public record ModifyWorkflowCommand(WorkflowId WorkflowId, ModificationPayload Payload) : IRequest<Result<Unit>>;

public class ModifyWorkflowCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory)
    : IRequestHandler<ModifyWorkflowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ModifyWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();
        
        Workflow? workflow = await dbContext.Workflows
            .Include(w => w.WorkflowTemplate)
            .Where(x => x.Id == request.WorkflowId.Value)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (workflow == null)
            return Result<Unit>.Failure($"Workflow with ID {request.WorkflowId.Value} not found.");
        
        IWorkflowMachine? stateMachine = workflowFactory.TryGetStateMachine(workflow.WorkflowTemplate.WorkflowBaseId);
        if (stateMachine == null) 
            return Result<Unit>.Failure("Invalid workflow machine type.");
        
        Result<Unit> reHydrationResult = stateMachine.RehydrateAsync(workflow, cancellationToken);
        if (!reHydrationResult.Succeeded)
            return Result<Unit>.Failure("Workflow state-machine reHydration failed.");
        
        return await stateMachine.ModifyAsync(request.Payload, cancellationToken);
    }
}