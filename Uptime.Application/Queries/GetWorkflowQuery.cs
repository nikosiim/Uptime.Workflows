using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Entities;

namespace Uptime.Application.Queries;

public record GetWorkflowQuery(int WorkflowId) : IRequest<Workflow?>;

public class GetWorkflowQueryHandler(IWorkflowDbContext dbContext) : IRequestHandler<GetWorkflowQuery, Workflow?>
{
    public async Task<Workflow?> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows.FirstOrDefaultAsync(wi => wi.Id == request.WorkflowId, cancellationToken: cancellationToken);
    }
}