using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Application.Queries;

public record GetWorkflowTemplateQuery(WorkflowTemplateId TemplateId) : IRequest<WorkflowTemplateDto?>;

public class GetWorkflowTemplateQueryHandler(WorkflowDbContext context) 
    : IRequestHandler<GetWorkflowTemplateQuery, WorkflowTemplateDto?>
{
    public async Task<WorkflowTemplateDto?> Handle(GetWorkflowTemplateQuery request, CancellationToken cancellationToken)
    {
        WorkflowTemplateDto? workflowTemplate = await context.WorkflowTemplates.AsNoTracking()
            .Where(wt => wt.Id == request.TemplateId.Value)
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