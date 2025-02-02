using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Queries;

public record GetWorkflowTemplateQuery(int TemplateId) : IRequest<WorkflowTemplateDto?>;

public class GetWorkflowTemplateQueryHandler(IWorkflowDbContext context) 
    : IRequestHandler<GetWorkflowTemplateQuery, WorkflowTemplateDto?>
{
    public async Task<WorkflowTemplateDto?> Handle(GetWorkflowTemplateQuery request, CancellationToken cancellationToken)
    {
        WorkflowTemplateDto? workflowTemplate = await context.WorkflowTemplates
            .Where(wt => wt.Id == request.TemplateId)
            .Select(wt => new WorkflowTemplateDto
            {
                Id = wt.Id,
                Name = wt.TemplateName,
                AssociationDataJson = wt.AssociationDataJson,
                WorkflowBaseId = wt.WorkflowBaseId,
                Created = wt.Created
            })
            .FirstOrDefaultAsync(cancellationToken);

        return workflowTemplate;
    }
}