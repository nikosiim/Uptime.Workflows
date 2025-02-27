using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;

namespace Uptime.Application.Commands;

public record DeleteWorkflowTemplateCommand(WorkflowTemplateId TemplateId) : IRequest;

public class DeleteWorkflowTemplateCommandHandler(IWorkflowDbContext context, ILogger<DeleteWorkflowTemplateCommand> logger)
    : IRequestHandler<DeleteWorkflowTemplateCommand>
{
    public async Task Handle(DeleteWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        WorkflowTemplate? template = await context.WorkflowTemplates.FirstOrDefaultAsync(t => t.Id == request.TemplateId.Value, cancellationToken);
        if (template == null) 
            return;

        bool hasWorkflows = await context.Workflows.AnyAsync(w => w.WorkflowTemplateId == request.TemplateId.Value, cancellationToken: cancellationToken);
        if (hasWorkflows)
        {
            throw new InvalidOperationException("Cannot delete the template as it is being used by existing workflows.");
        }

        template.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Workflow template with ID {TemplateId} deleted.", request.TemplateId);
    }
}