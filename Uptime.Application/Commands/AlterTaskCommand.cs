using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Commands;

public record AlterTaskCommand : IRequest<string>
{
    public TaskId TaskId { get; init; }
    public WorkflowId WorkflowId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
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

        if (workflowTask is null) 
            return WorkflowPhase.Invalid.Value;
        
        if (!Guid.TryParse(workflowTask.Workflow.WorkflowTemplate.WorkflowBaseId, out Guid workflowBaseId))
        {
            return WorkflowPhase.Invalid.Value;
        }
        
        IWorkflowMachine? workflow = workflowFactory.GetWorkflow(workflowBaseId);
        if (workflow is not IActivityWorkflowMachine machine)
        {
            logger.LogWarning("The workflow with ID {WorkflowBaseId} does not support task alterations.", workflowBaseId);
            return WorkflowPhase.Invalid.Value;
        }

        bool isRehydrated = await machine.RehydrateAsync(request.WorkflowId, cancellationToken);
        if (!isRehydrated)
        {
            return WorkflowPhase.Invalid.Value;
        }
        
        var taskContext = new WorkflowTaskContext((WorkflowId)workflowTask.WorkflowId)
        {
            TaskId = request.TaskId,
            TaskGuid = workflowTask.TaskGuid,
            AssignedTo = workflowTask.AssignedTo,
            AssignedBy = workflowTask.AssignedBy,
            TaskDescription = workflowTask.Description,
            DueDate = workflowTask.DueDate,
            Storage = workflowTask.StorageJson.DeserializeStorage()
        };

        await machine.AlterTaskCoreAsync(taskContext, cancellationToken);

        return machine.CurrentState.Value;
    }
}