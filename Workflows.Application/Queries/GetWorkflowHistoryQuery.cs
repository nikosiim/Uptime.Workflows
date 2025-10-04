using Microsoft.EntityFrameworkCore;
using Workflows.Application.DTOs;
using Workflows.Application.Messaging;
using Workflows.Core.Common;
using Workflows.Core.Data;

namespace Workflows.Application.Queries;

public record GetWorkflowHistoryQuery(WorkflowId WorkflowId) : IRequest<List<WorkflowHistoryDto>>;

public class GetWorkflowHistoryQueryHandler(WorkflowDbContext db) 
    : IRequestHandler<GetWorkflowHistoryQuery, List<WorkflowHistoryDto>>
{
    public async Task<List<WorkflowHistoryDto>> Handle(GetWorkflowHistoryQuery request, CancellationToken ct)
    {
        return await db.WorkflowHistories.AsNoTracking()
            .Where(x => x.WorkflowId == request.WorkflowId.Value)
            .Select(history => new WorkflowHistoryDto
            {
                Id = history.Id,
                WorkflowId = history.WorkflowId,
                Description = history.Description,
                Event = history.Event,
                Occurred = history.Occurred,
                Comment = history.Comment,
                ExecutedBy = history.PerformedBy.Name
            })
            .ToListAsync(ct);
    }
}