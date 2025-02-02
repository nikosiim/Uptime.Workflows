using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Entities;

namespace Uptime.Application.Commands;

public record UpdateWorkflowTemplateCommand : IRequest<bool>
{
    public int TemplateId { get; init; }
    public string TemplateName { get; init; } = null!;
    public string WorkflowName { get; init; } = null!;
    public string WorkflowBaseId { get; init; } = null!;
    public string? AssociationDataJson { get; init; }
}

public class UpdateWorkflowTemplateCommandHandler(IWorkflowDbContext context)
    : IRequestHandler<UpdateWorkflowTemplateCommand, bool>
{
    public async Task<bool> Handle(UpdateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        WorkflowTemplate? workflowTemplate = await context.WorkflowTemplates
            .FirstOrDefaultAsync(wt => wt.Id == request.TemplateId, cancellationToken);

        if (workflowTemplate == null)
        {
            throw new NotFoundException(nameof(WorkflowTemplate), request.TemplateId);
        }

        workflowTemplate.TemplateName = request.TemplateName;
        workflowTemplate.WorkflowName = request.WorkflowName;
        workflowTemplate.WorkflowBaseId = request.WorkflowBaseId;
        workflowTemplate.AssociationDataJson = request.AssociationDataJson;
        workflowTemplate.Modified = DateTime.UtcNow;

        context.WorkflowTemplates.Update(workflowTemplate);
        return await context.SaveChangesAsync(cancellationToken) == 1;
    }
}