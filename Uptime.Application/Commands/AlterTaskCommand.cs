using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Domain.Common;
using Uptime.Domain.Data;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;
using Unit = Uptime.Domain.Common.Unit;

namespace Uptime.Application.Commands;

public record AlterTaskCommand : IRequest<Result<Unit>>
{
    public TaskId TaskId { get; init; }
    public Dictionary<string, string?> Payload { get; init; } = new();
}

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