using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public sealed record AlterTaskCommand : IRequest<Result<Unit>>, IPrincipalRequest
{
    public required TaskId TaskId { get; init; }
    public required string ExecutorSid { get; init; }
    public required WorkflowEventType Action { get; init; }
    public required Dictionary<string, string?> Payload { get; init; }

    // Will be populated by pipeline
    public Principal ExecutedBy { get; set; } = null!;
}

public sealed class AlterTaskCommandHandler(WorkflowDbContext db, IWorkflowFactory workflowFactory, ILogger<AlterTaskCommandHandler> log)
    : IRequestHandler<AlterTaskCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AlterTaskCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        WorkflowTask? task = await db.WorkflowTasks
            .Include(x => x.Workflow)
            .ThenInclude(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(task => task.Id == request.TaskId.Value, ct);

        if (task is null)
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow task {request.TaskId.Value} not found.");

        IWorkflowMachine? sm = workflowFactory.TryGetStateMachine(task.Workflow.WorkflowTemplate.WorkflowBaseId);
        if (sm is not IActivityWorkflowMachine machine)
        {
            log.LogError("Invalid state-machine type {StateMachineType}", sm?.GetType());
            return Result<Unit>.Failure(ErrorCode.Unsupported);
        }
      
        Result<Unit> rehydrationResult = machine.Rehydrate(task.Workflow.StorageJson!, task.Workflow.Phase, ct);
        if (!rehydrationResult.Succeeded)
            return rehydrationResult;
        
        var input = new AlterTaskPayload
        {
            ExecutedBy = request.ExecutedBy,
            PhaseId = task.PhaseId,
            TaskGuid = task.TaskGuid,
            AssignedTo = (PrincipalId)task.AssignedToPrincipalId,
            DueDate = task.DueDate,
            Description = task.Description,
            StorageJson = task.StorageJson,
            InputData = request.Payload
        };
        
        return await machine.AlterTaskAsync(request.Action, input, ct);
    }
}