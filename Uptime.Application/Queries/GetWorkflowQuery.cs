using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Queries;

public record GetWorkflowQuery(WorkflowId WorkflowId) : IRequest<WorkflowDto?>;

public class GetWorkflowQueryHandler(IWorkflowDbContext dbContext) : IRequestHandler<GetWorkflowQuery, WorkflowDto?>
{
    public async Task<WorkflowDto?> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows
            .Where(x => x.Id == request.WorkflowId.Value)
            .Select(w => new WorkflowDto
            {
                IsActive = w.IsActive,
                Outcome = w.Outcome,
                Phase = w.Phase,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                Originator = w.Originator,
                InstanceDataJson = w.StorageJson,
                DocumentId = w.DocumentId,
                Document = w.Document.Title
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}