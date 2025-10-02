using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Queries;

public record GetLibraryWorkflowTemplatesQuery(Guid ListId) : IRequest<List<LibraryWorkflowTemplateDto>>;

public class GetLibraryWorkflowTemplatesQueryHandler(WorkflowDbContext db) 
    : IRequestHandler<GetLibraryWorkflowTemplatesQuery, List<LibraryWorkflowTemplateDto>>
{
    public async Task<List<LibraryWorkflowTemplateDto>> Handle(GetLibraryWorkflowTemplatesQuery request, CancellationToken ct)
    {
        return await db.WorkflowTemplates.AsNoTracking()
            .Where(w => w.LibraryId == request.ListId && !w.IsDeleted)
            .Select(w => new LibraryWorkflowTemplateDto
            {
                Id = w.Id,
                Name = w.TemplateName,
                WorkflowBaseId = w.WorkflowBaseId,
                AssociationDataJson = w.AssociationDataJson,
                Created = w.CreatedAtUtc.UtcDateTime
            })
            .ToListAsync(ct);
    }
}