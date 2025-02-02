using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Queries;

public record GetLibraryQuery(int LibraryId) : IRequest<LibraryDto?>;

public class GetLibraryQueryHandler(IWorkflowDbContext context) : IRequestHandler<GetLibraryQuery, LibraryDto?>
{
    public async Task<LibraryDto?> Handle(GetLibraryQuery request, CancellationToken cancellationToken)
    {
        return await context.Libraries
            .Where(l => l.Id == request.LibraryId)
            .Select(l => new LibraryDto(l.Id, l.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }
}