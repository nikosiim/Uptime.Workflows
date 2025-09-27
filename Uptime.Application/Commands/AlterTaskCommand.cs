using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public sealed record AlterTaskCommand : IRequest<Result<Unit>>, IRequiresPrincipal
{
    public required Guid TaskGuid { get; init; }
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
            .FirstOrDefaultAsync(task => task.TaskGuid == request.TaskGuid, ct);

        if (task is null)
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow task {request.TaskGuid} not found.");

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
            TaskGuid = task.TaskGuid,
            ExecutedBy = request.ExecutedBy,
            PhaseId = task.PhaseId,
            AssignedTo = (PrincipalId)task.AssignedToPrincipalId,
            DueDate = task.DueDate,
            Description = task.Description,
            StorageJson = task.StorageJson,
            InputData = request.Payload
        };
        
        return await machine.AlterTaskAsync(request.Action, input, ct);
    }
}