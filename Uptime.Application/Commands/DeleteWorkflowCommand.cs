using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;

namespace Uptime.Application.Commands;

public record DeleteWorkflowCommand(WorkflowId WorkflowId) : IRequest;

public class DeleteWorkflowCommandHandler(IWorkflowDbContext dbContext, ILogger<DeleteWorkflowCommand> logger)
    : IRequestHandler<DeleteWorkflowCommand>
{
    public async Task Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        Workflow? workflow = await dbContext.Workflows
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, cancellationToken);

        if (workflow != null)
        {
            workflow.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Workflow with ID {WorkflowId} deleted.", request.WorkflowId);
        }
    }
}