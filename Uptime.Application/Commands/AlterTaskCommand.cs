using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Commands;

public record AlterTaskCommand : IRequest<WorkflowPhase>
{
    public TaskId TaskId { get; init; }
    public WorkflowId WorkflowId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class AlterTaskCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory, ILogger<AlterTaskCommand> logger) 
    : IRequestHandler<AlterTaskCommand, WorkflowPhase>
{
    public async Task<WorkflowPhase> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        WorkflowTask? workflowTask = dbContext.WorkflowTasks
            .Include(x => x.Workflow)
            .ThenInclude(w => w.WorkflowTemplate)
            .FirstOrDefault(task => task.Id == request.TaskId.Value);

        if (workflowTask is null) 
            return WorkflowPhase.Invalid;
        
        if (!Guid.TryParse(workflowTask.Workflow.WorkflowTemplate.WorkflowBaseId, out Guid workflowBaseId))
        {
            return WorkflowPhase.Invalid;
        }
        
        IWorkflowMachine? workflow = workflowFactory.GetWorkflow(workflowBaseId);
        if (workflow is not IActivityWorkflowMachine machine)
        {
            logger.LogWarning("The workflow with ID {WorkflowBaseId} does not support task alterations.", workflowBaseId);
            return WorkflowPhase.Invalid;
        }

        bool isRehydrated = await machine.RehydrateAsync(request.WorkflowId, cancellationToken);
        if (!isRehydrated)
        {
            return WorkflowPhase.Invalid;
        }
        
        var taskContext = new WorkflowTaskContext((WorkflowId)workflowTask.WorkflowId)
        {
            TaskId = request.TaskId,
            TaskGuid = workflowTask.TaskGuid,
            AssignedTo = workflowTask.AssignedTo,
            AssignedBy = workflowTask.AssignedBy,
            TaskDescription = workflowTask.Description,
            DueDate = workflowTask.DueDate,
            Storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(workflowTask.StorageJson ?? "{}") ?? new Dictionary<string, string?>()
        };

        await machine.AlterTaskCoreAsync(taskContext);

        return machine.CurrentState;
    }
}