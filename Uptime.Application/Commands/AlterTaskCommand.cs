using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Uptime.Workflows.Application.Authentication;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record AlterTaskCommand(ClaimsPrincipal Caller, TaskId TaskId, Dictionary<string, string?> Payload)
    : IRequest<Result<Unit>>, ITaskAuthorizationRequest;

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
            return Result<Unit>.Failure(ErrorCode.NotFound);

        IWorkflowMachine? sm = workflowFactory.TryGetStateMachine(task.Workflow.WorkflowTemplate.WorkflowBaseId);
        if (sm is not IActivityWorkflowMachine machine)
        {
            log.LogError("Invalid state-machine type {StateMachineType}", sm?.GetType());
            return Result<Unit>.Failure(ErrorCode.Unsupported);
        }
      
        Result<Unit> rehydrationResult = machine.Rehydrate(task.Workflow, ct);
        if (!rehydrationResult.Succeeded)
            return rehydrationResult;
        
        return await machine.AlterTaskAsync(task, request.Payload, ct);
    }
}