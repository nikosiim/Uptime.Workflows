using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;

namespace Uptime.Application.Commands;

public record DeleteWorkflowTemplateCommand(WorkflowTemplateId TemplateId) : IRequest<bool>;

public class DeleteWorkflowTemplateCommandHandler(IWorkflowDbContext context)
    : IRequestHandler<DeleteWorkflowTemplateCommand, bool>
{
    public async Task<bool> Handle(DeleteWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        WorkflowTemplate? template = await context.WorkflowTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId.Value, cancellationToken);
        
        if (template == null) 
            return false;

        bool hasWorkflows = await context.Workflows.AnyAsync(w => w.WorkflowTemplateId == request.TemplateId.Value, cancellationToken: cancellationToken);
        if (hasWorkflows)
        {
            throw new InvalidOperationException("Cannot delete the template as it is being used by existing workflows.");
        }

        context.WorkflowTemplates.Remove(template);
        return await context.SaveChangesAsync(cancellationToken) == 1;
    }
}