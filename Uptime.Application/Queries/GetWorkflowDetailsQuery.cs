using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Queries;

public record GetWorkflowDetailsQuery(WorkflowId WorkflowId) : IRequest<WorkflowDetailsDto?>;

public class GetWorkflowDetailsQueryHandler(IWorkflowDbContext dbContext) : IRequestHandler<GetWorkflowDetailsQuery, WorkflowDetailsDto?>
{
    public async Task<WorkflowDetailsDto?> Handle(GetWorkflowDetailsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows.AsNoTracking()
            .Where(x => x.Id == request.WorkflowId.Value)
            .Select(w => new WorkflowDetailsDto
            {
                IsActive = w.IsActive,
                Outcome =  w.Outcome,
                Phase = w.Phase,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                Originator = w.Originator,
                DocumentId = w.DocumentId,
                Document = w.Document.Title,
                WorkflowBaseId = w.WorkflowTemplate.WorkflowBaseId
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}