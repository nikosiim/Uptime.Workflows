using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Domain.Common;
using Uptime.Domain.Data;

namespace Uptime.Application.Queries;

public record GetDocumentWorkflowsQuery(DocumentId DocumentId) : IRequest<List<DocumentWorkflowDto>>;

public class GetDocumentWorkflowsQueryHandler(WorkflowDbContext dbContext)
    : IRequestHandler<GetDocumentWorkflowsQuery, List<DocumentWorkflowDto>>
{
    public async Task<List<DocumentWorkflowDto>> Handle(GetDocumentWorkflowsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows.AsNoTracking()
            .Where(w => w.DocumentId == request.DocumentId.Value && !w.IsDeleted)
            .Select(w => new DocumentWorkflowDto
            {
                Id = w.Id,
                TemplateId = w.WorkflowTemplateId,
                WorkflowTemplateName = w.WorkflowTemplate.TemplateName,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                Outcome = w.Outcome,
                IsActive = w.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}