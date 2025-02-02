using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Queries;

public record GetDocumentWorkflowsQuery(int DocumentId) : IRequest<List<DocumentWorkflowDto>>;

public class GetDocumentWorkflowsQueryHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<GetDocumentWorkflowsQuery, List<DocumentWorkflowDto>>
{
    public async Task<List<DocumentWorkflowDto>> Handle(GetDocumentWorkflowsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows
            .Where(wi => wi.DocumentId == request.DocumentId)
            .Include(wi => wi.WorkflowTemplate)
            .Select(wi => new DocumentWorkflowDto
            {
                Id = wi.Id,
                TemplateId = wi.WorkflowTemplateId,
                WorkflowTemplateName = wi.WorkflowTemplate != null ? wi.WorkflowTemplate.TemplateName : "Template Missing",
                StartDate = wi.StartDate,
                EndDate = wi.EndDate,
                Status = wi.Status
            })
            .ToListAsync(cancellationToken);
    }
}