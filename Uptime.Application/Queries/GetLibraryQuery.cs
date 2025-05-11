using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Queries;

public record GetLibraryQuery(LibraryId LibraryId) : IRequest<Result<LibraryDto>>;

public class GetLibraryQueryHandler(WorkflowDbContext db) 
    : IRequestHandler<GetLibraryQuery, Result<LibraryDto>>
{
    public async Task<Result<LibraryDto>> Handle(GetLibraryQuery request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<LibraryDto>.Cancelled();

        LibraryDto? dto = await db.Libraries.AsNoTracking()
            .Where(l => l.Id == request.LibraryId.Value && !l.IsDeleted)
            .Select(l => new LibraryDto(l.Id, l.Name))
            .FirstOrDefaultAsync(ct);

        return dto is not null
            ? Result<LibraryDto>.Success(dto)
            : Result<LibraryDto>.Failure(ErrorCode.NotFound);
    }
}