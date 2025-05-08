using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record CancelWorkflowCommand(WorkflowId WorkflowId, string Executor, string Comment) : IRequest<Result<Unit>>;

public class CancelWorkflowCommandHandler(WorkflowDbContext dbContext, IWorkflowFactory workflowFactory)
    : IRequestHandler<CancelWorkflowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        Workflow? workflow = await dbContext.Workflows
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, cancellationToken);
        
        if (workflow == null)
            return Result<Unit>.Failure($"Workflow with ID {request.WorkflowId.Value} not found.");
        
        IWorkflowMachine? stateMachine = workflowFactory.TryGetStateMachine(workflow.WorkflowTemplate.WorkflowBaseId);
        if (stateMachine == null) 
            return Result<Unit>.Failure("Invalid workflow machine type.");
        
        Result<Unit> reHydrationResult = stateMachine.RehydrateAsync(workflow, cancellationToken);
        if (!reHydrationResult.Succeeded)
            return Result<Unit>.Failure("Workflow state-machine reHydration failed.");

        return await stateMachine.CancelAsync(request.Executor, request.Comment, cancellationToken);
    }
}