using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Queries;

public record GetWorkflowQuery(int WorkflowId) : IRequest<WorkflowDto?>;

public class GetWorkflowQueryHandler(IWorkflowDbContext dbContext) : IRequestHandler<GetWorkflowQuery, WorkflowDto?>
{
    public async Task<WorkflowDto?> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows
            .Where(x => x.Id == request.WorkflowId)
            .Select(w => new WorkflowDto
            {
                Status = w.Status,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                Originator = w.Originator,
                InstanceDataJson = w.InstanceDataJson,
                DocumentId = w.DocumentId,
                Document = w.Document.Title
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}