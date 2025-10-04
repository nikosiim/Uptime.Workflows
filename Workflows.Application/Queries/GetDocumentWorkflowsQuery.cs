using Microsoft.EntityFrameworkCore;
using Workflows.Application.DTOs;
using Workflows.Application.Messaging;
using Workflows.Core.Common;
using Workflows.Core.Data;

namespace Workflows.Application.Queries;

public record GetDocumentWorkflowsQuery(DocumentId DocumentId) : IRequest<List<DocumentWorkflowDto>>;

public class GetDocumentWorkflowsQueryHandler(WorkflowDbContext db)
    : IRequestHandler<GetDocumentWorkflowsQuery, List<DocumentWorkflowDto>>
{
    public async Task<List<DocumentWorkflowDto>> Handle(GetDocumentWorkflowsQuery request, CancellationToken ct)
    {
        return await db.Workflows.AsNoTracking()
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
            .ToListAsync(ct);
    }
}