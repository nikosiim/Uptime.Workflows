using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Commands;

public record AlterTaskCommand : IRequest<string>
{
    public TaskId TaskId { get; init; }
    public Dictionary<string, string?> Payload { get; init; } = new();
}

public class AlterTaskCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory, ILogger<AlterTaskCommand> logger) 
    : IRequestHandler<AlterTaskCommand, string>
{
    public async Task<string> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        WorkflowTask? workflowTask = await dbContext.WorkflowTasks
            .Include(x => x.Workflow)
            .ThenInclude(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(task => task.Id == request.TaskId.Value, cancellationToken);

        if (workflowTask == null)
            throw new InvalidOperationException($"Workflow task with ID {request.TaskId.Value} not found.");
        
        var baseId = new Guid(workflowTask.Workflow.WorkflowTemplate.WorkflowBaseId);
        
        IWorkflowMachine? stateMachine = workflowFactory.TryGetStateMachine(baseId);
        if (stateMachine is not IActivityWorkflowMachine machine)
        {
            logger.LogWarning("The workflow with ID {WorkflowBaseId} does not support task alterations.", baseId);
            return BaseState.Invalid.Value;
        }

        if (!await machine.RehydrateAsync(workflowTask.Workflow, cancellationToken))
        {
            return BaseState.Invalid.Value;
        }
        
        var taskContext = new WorkflowTaskContext(workflowTask);
        await machine.AlterTaskAsync(taskContext, request.Payload, cancellationToken);

        return machine.CurrentState.Value;
    }
}