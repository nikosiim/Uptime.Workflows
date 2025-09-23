using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Queries;

public record GetLibraryDocumentsQuery(LibraryId LibraryId) : IRequest<List<LibraryDocumentDto>>;

public class GetLibraryDocumentsQueryHandler(WorkflowDbContext db)
    : IRequestHandler<GetLibraryDocumentsQuery, List<LibraryDocumentDto>>
{
    public async Task<List<LibraryDocumentDto>> Handle(GetLibraryDocumentsQuery request, CancellationToken ct)
    {
        return await db.Documents.AsNoTracking()
            .Where(d => d.Library.Id == request.LibraryId.Value)
            .Select(wt => new LibraryDocumentDto
            {
                Id = wt.Id,
                Title = wt.Title,
                Description = wt.Description,
                LibraryId = wt.LibraryId
            })
            .ToListAsync(ct);
    }
}