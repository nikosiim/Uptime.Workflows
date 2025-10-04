using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Workflows.Application.Messaging;
using Workflows.Core.Common;
using Workflows.Core.Data;
using Workflows.Core.Enums;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace Workflows.Application.Commands;

public sealed record AlterTaskCommand : IRequest<Result<Unit>>, IRequiresPrincipal
{
    public required Guid TaskGuid { get; init; }
    public required PrincipalSid ExecutorSid { get; init; }
    public required WorkflowEventType Action { get; init; }
    public required Dictionary<string, string?> Payload { get; init; }
}

public sealed class AlterTaskCommandHandler(WorkflowDbContext db, IWorkflowFactory workflowFactory, ILogger<AlterTaskCommandHandler> log)
    : IRequestHandler<AlterTaskCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AlterTaskCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        WorkflowTask? task = await db.WorkflowTasks
            .AsNoTracking()
            .Include(t => t.AssignedTo)
            .Include(t => t.Workflow)
            .ThenInclude(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(t => t.TaskGuid == request.TaskGuid, ct);

        if (task is null)
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow task {request.TaskGuid} not found.");

        IWorkflowMachine? sm = workflowFactory.TryGetStateMachine(task.Workflow.WorkflowTemplate.WorkflowBaseId);
        if (sm is not IActivityWorkflowMachine machine)
        {
            log.LogError("Invalid state-machine type {StateMachineType}", sm?.GetType());
            return Result<Unit>.Failure(ErrorCode.Unsupported);
        }
      
        Result<Unit> rehydrationResult = await machine.Rehydrate(task.Workflow.StorageJson!, task.Workflow.Phase, ct);
        if (!rehydrationResult.Succeeded)
            return rehydrationResult;
        
        var input = new UpdateTaskPayload
        {
            TaskGuid = task.TaskGuid,
            ExecutorSid = request.ExecutorSid,
            PhaseId = task.PhaseId,
            AssignedToSid = (PrincipalSid)task.AssignedTo.ExternalId,
            DueDate = task.DueDate,
            Description = task.Description,
            StorageJson = task.StorageJson,
            InputData = request.Payload
        };
        
        return await machine.AlterTaskAsync(request.Action, input, ct);
    }
}