using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record CancelWorkflowCommand : IRequest<Result<Unit>>, IPrincipalRequest
{
    public required string CallerSid { get; init; }
    public required WorkflowId WorkflowId { get; init; }
    public required string Comment { get; init; }

    // Will be populated by pipeline
    public Principal? Caller { get; set; } 
};

public class CancelWorkflowCommandHandler(WorkflowDbContext db, IWorkflowFactory factory, ILogger<CancelWorkflowCommand> log)
    : IRequestHandler<CancelWorkflowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CancelWorkflowCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        Workflow? workflow = await db.Workflows
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, ct);
        
        if (workflow == null)
            return Result<Unit>.Failure(ErrorCode.NotFound, $"Workflow with ID {request.WorkflowId.Value} not found.");
        
        IWorkflowMachine? machine = factory.TryGetStateMachine(workflow.WorkflowTemplate.WorkflowBaseId);
        if (machine == null)
        {
            log.LogError("State-machine not found {WorkflowBaseId}", workflow.WorkflowTemplate.WorkflowBaseId);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }
        
        Result<Unit> rehydrationResult = machine.Rehydrate(workflow, ct);
        if (!rehydrationResult.Succeeded)
            return rehydrationResult;

        Principal principal = request.Caller!;
        
        return await machine.CancelAsync(principal.Id, request.Comment, ct);
    }
}