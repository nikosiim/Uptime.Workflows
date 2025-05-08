using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record AlterTaskCommand(ClaimsPrincipal User, TaskId TaskId, Dictionary<string, string?> Payload)
    : IRequest<Result<Unit>>;

public class AlterTaskCommandHandler(WorkflowDbContext dbContext, IWorkflowFactory workflowFactory) 
    : IRequestHandler<AlterTaskCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        WorkflowTask? workflowTask = await dbContext.WorkflowTasks
            .Include(x => x.Workflow)
            .ThenInclude(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(task => task.Id == request.TaskId.Value, cancellationToken);

        if (workflowTask == null)
            return Result<Unit>.Failure($"Workflow task with ID {request.TaskId.Value} not found.");

        IWorkflowMachine? stateMachine = workflowFactory.TryGetStateMachine(workflowTask.Workflow.WorkflowTemplate.WorkflowBaseId);
        if (stateMachine is not IActivityWorkflowMachine machine) 
            return Result<Unit>.Failure("Invalid workflow machine type.");
      
        Result<Unit> reHydrationResult = machine.RehydrateAsync(workflowTask.Workflow, cancellationToken);
        if (!reHydrationResult.Succeeded)
            return Result<Unit>.Failure("Workflow state-machine reHydration failed.");
        
        return await machine.AlterTaskAsync(workflowTask, request.Payload, cancellationToken);
    }
}