using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Commands;

public record AlterTaskCommand : IRequest<WorkflowPhase>
{
    public TaskId TaskId { get; init; }
    public WorkflowId WorkflowId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class AlterTaskCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory) 
    : IRequestHandler<AlterTaskCommand, WorkflowPhase>
{
    public async Task<WorkflowPhase> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        string workflowBaseIdString = await dbContext.Workflows
            .Where(x => x.Id == request.WorkflowId.Value)
            .Select(w => w.WorkflowTemplate.WorkflowBaseId).FirstAsync(cancellationToken);
        
        if (!Guid.TryParse(workflowBaseIdString, out Guid workflowBaseId))
        {
            return WorkflowPhase.Invalid;
        }

        IWorkflowMachine? workflow = workflowFactory.GetWorkflow(workflowBaseId);
        if (workflow is null)
        {
            return WorkflowPhase.Invalid;
        }

        bool isRehydrated = await workflow.RehydrateAsync(request.WorkflowId, cancellationToken);
        if (!isRehydrated)
        {
            return WorkflowPhase.Invalid;
        }

        var payload = new AlterTaskPayload(request.TaskId, request.WorkflowId, request.Storage);

        return await workflow.AlterTaskCoreAsync(payload);
    }
}