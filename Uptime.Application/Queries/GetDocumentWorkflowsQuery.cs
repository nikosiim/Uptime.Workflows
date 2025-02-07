using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Queries;

public record GetDocumentWorkflowsQuery(DocumentId DocumentId) : IRequest<List<DocumentWorkflowDto>>;

public class GetDocumentWorkflowsQueryHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<GetDocumentWorkflowsQuery, List<DocumentWorkflowDto>>
{
    public async Task<List<DocumentWorkflowDto>> Handle(GetDocumentWorkflowsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows
            .Where(w => w.DocumentId == request.DocumentId.Value)
            .Select(w => new DocumentWorkflowDto
            {
                Id = w.Id,
                TemplateId = w.WorkflowTemplateId,
                WorkflowTemplateName = w.WorkflowTemplate.TemplateName,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                Status = w.Status
            })
            .ToListAsync(cancellationToken);
    }
}