using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Application.Queries;

public record GetLibraryQuery(LibraryId LibraryId) : IRequest<LibraryDto?>;

public class GetLibraryQueryHandler(WorkflowDbContext context) : IRequestHandler<GetLibraryQuery, LibraryDto?>
{
    public async Task<LibraryDto?> Handle(GetLibraryQuery request, CancellationToken cancellationToken)
    {
        return await context.Libraries.AsNoTracking()
            .Where(l => l.Id == request.LibraryId.Value && !l.IsDeleted)
            .Select(l => new LibraryDto(l.Id, l.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }
}