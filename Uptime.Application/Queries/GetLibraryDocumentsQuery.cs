using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Queries;

public record GetLibraryDocumentsQuery(LibraryId LibraryId) : IRequest<List<LibraryDocumentDto>>;

public class GetLibraryDocumentsQueryHandler(IWorkflowDbContext context)
    : IRequestHandler<GetLibraryDocumentsQuery, List<LibraryDocumentDto>>
{
    public async Task<List<LibraryDocumentDto>> Handle(GetLibraryDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await context.Documents.AsNoTracking()
            .Where(d => d.Library.Id == request.LibraryId.Value)
            .Select(wt => new LibraryDocumentDto
            {
                Id = wt.Id,
                Title = wt.Title,
                Description = wt.Description,
                LibraryId = wt.LibraryId
            })
            .ToListAsync(cancellationToken);
    }
}