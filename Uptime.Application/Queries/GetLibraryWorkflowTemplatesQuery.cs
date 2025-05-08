using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Queries;

public record GetLibraryWorkflowTemplatesQuery(LibraryId ListId) : IRequest<List<LibraryWorkflowTemplateDto>>;

public class GetLibraryWorkflowTemplatesQueryHandler(WorkflowDbContext dbContext) : IRequestHandler<GetLibraryWorkflowTemplatesQuery, List<LibraryWorkflowTemplateDto>>
{
    public async Task<List<LibraryWorkflowTemplateDto>> Handle(GetLibraryWorkflowTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowTemplates.AsNoTracking()
            .Where(w => w.LibraryId == request.ListId.Value && !w.IsDeleted)
            .Select(w => new LibraryWorkflowTemplateDto
            {
                Id = w.Id,
                Name = w.TemplateName,
                WorkflowBaseId = w.WorkflowBaseId,
                AssociationDataJson = w.AssociationDataJson,
                Created = w.Created
            })
            .ToListAsync(cancellationToken);
    }
}