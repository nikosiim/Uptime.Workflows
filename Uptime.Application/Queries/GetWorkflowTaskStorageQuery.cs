using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Queries;

public record GetWorkflowTaskStorageQuery(TaskId TaskId) : IRequest<string?>;

public class GetWorkflowTaskStorageQueryHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<GetWorkflowTaskStorageQuery, string?>
{
    public async Task<string?> Handle(GetWorkflowTaskStorageQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowTasks
            .Where(t => t.Id == request.TaskId.Value)
            .Select(t => t.StorageJson)
            .FirstOrDefaultAsync(cancellationToken);
    }
}