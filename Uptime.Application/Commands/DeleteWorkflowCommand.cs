using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Commands;

public record DeleteWorkflowCommand(WorkflowId WorkflowId) : IRequest;

public class DeleteWorkflowCommandHandler(WorkflowDbContext dbContext, ILogger<DeleteWorkflowCommand> logger)
    : IRequestHandler<DeleteWorkflowCommand>
{
    public async Task Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        Workflow? workflow = await dbContext.Workflows
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, cancellationToken);

        if (workflow != null)
        {
            dbContext.Workflows.Remove(workflow);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Workflow [{WorkflowId}] - Workflow deleted.", request.WorkflowId);
        }
    }
}