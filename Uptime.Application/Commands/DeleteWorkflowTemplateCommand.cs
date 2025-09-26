using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record DeleteWorkflowTemplateCommand(WorkflowTemplateId TemplateId) : IRequest<Result<Unit>>;

public class DeleteWorkflowTemplateCommandHandler(WorkflowDbContext db, ILogger<DeleteWorkflowTemplateCommand> logger)
    : IRequestHandler<DeleteWorkflowTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteWorkflowTemplateCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        WorkflowTemplate? template = await db.WorkflowTemplates.FirstOrDefaultAsync(t => t.Id == request.TemplateId.Value, ct);
        if (template == null)
            return Result<Unit>.Failure(ErrorCode.NotFound);

        bool inUse = await db.Workflows.AnyAsync(w => w.WorkflowTemplateId == request.TemplateId.Value && w.IsActive, ct);
        if (inUse)
        {
            return Result<Unit>.Failure(ErrorCode.Conflict, "Cannot delete template: it’s used by existing workflows.");
        }

        template.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Workflow template with ID {TemplateId} deleted.", request.TemplateId.Value);
        
        return Result<Unit>.Success(new Unit());
    }
}