using Uptime.Workflows.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Commands;

public record DeleteWorkflowCommand(WorkflowId WorkflowId) : IRequest<Unit>;

public class DeleteWorkflowCommandHandler(WorkflowDbContext db, ILogger<DeleteWorkflowCommand> logger) 
    : IRequestHandler<DeleteWorkflowCommand, Unit>
{
    public async Task<Unit> Handle(DeleteWorkflowCommand request, CancellationToken ct)
    {
        Workflow? workflow = await db.Workflows
            .Include(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, ct);

        if (workflow != null)
        {
            db.Workflows.Remove(workflow);
            await db.SaveChangesAsync(ct);
            
            logger.LogInformation("Workflow [{WorkflowId}] - Workflow deleted.", request.WorkflowId);
        }

        return default;
    }
}