using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record ModifyWorkflowCommand(WorkflowId WorkflowId, ModificationPayload Payload) : IRequest<Result<Unit>>;

public class ModifyWorkflowCommandHandler(WorkflowDbContext dbContext, IWorkflowFactory workflowFactory)
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