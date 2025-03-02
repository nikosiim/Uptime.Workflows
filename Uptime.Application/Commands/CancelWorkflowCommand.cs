using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Commands;

public record CancelWorkflowCommand(WorkflowId WorkflowId, string Executor, string Comment) : IRequest;

public class CancelWorkflowCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory, ILogger<CancelWorkflowCommand> logger)
    : IRequestHandler<CancelWorkflowCommand>
{
    public async Task Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        Workflow? workflowInstance = await dbContext.Workflows
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, cancellationToken);

        if (workflowInstance == null)
        {
            logger.LogWarning("Workflow with ID {WorkflowId} not found.", request.WorkflowId);
            throw new NotFoundException(nameof(Workflow), request.WorkflowId);
        }
        
        if (!Guid.TryParse(workflowInstance.WorkflowTemplate.WorkflowBaseId, out Guid workflowBaseId))
        {
            logger.LogError("Invalid workflow base ID '{WorkflowBaseId}' for workflow {WorkflowId}.", 
                workflowInstance.WorkflowTemplate.WorkflowBaseId, 
                request.WorkflowId
            );
            throw new InvalidOperationException($"Invalid workflow base ID '{workflowInstance.WorkflowTemplate.WorkflowBaseId}'.");
        }
        
        IWorkflowMachine? machine = workflowFactory.TryGetStateMachine(workflowBaseId);
        if (machine == null)
        {
            logger.LogError("No workflow machine found for base ID {WorkflowBaseId}. Workflow: {WorkflowId}",
                workflowBaseId,
                request.WorkflowId
            );
            throw new InvalidOperationException($"No workflow machine found for base ID {workflowBaseId}.");
        }

        bool isRehydrated = await machine.RehydrateAsync(workflowInstance, cancellationToken);
        if (!isRehydrated)
        {
            logger.LogError("Failed to rehydrate workflow {WorkflowId} (possibly not found in DB).", request.WorkflowId);
            throw new InvalidOperationException($"Workflow {request.WorkflowId} could not be rehydrated.");
        }

        await machine.CancelWorkflowAsync(request.Executor, request.Comment, CancellationToken.None);
        logger.LogInformation("Successfully cancelled workflow {WorkflowId}.", request.WorkflowId);
    }
}