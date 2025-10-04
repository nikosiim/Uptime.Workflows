using Microsoft.EntityFrameworkCore;
using Workflows.Application.DTOs;
using Workflows.Application.Messaging;
using Workflows.Core.Common;
using Workflows.Core.Data;

namespace Workflows.Application.Queries;

public record GetWorkflowDetailsQuery(WorkflowId WorkflowId) : IRequest<Result<WorkflowDetailsDto>>;

public class GetWorkflowDetailsQueryHandler(WorkflowDbContext db) 
    : IRequestHandler<GetWorkflowDetailsQuery, Result<WorkflowDetailsDto>>
{
    public async Task<Result<WorkflowDetailsDto>> Handle(GetWorkflowDetailsQuery request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<WorkflowDetailsDto>.Cancelled();

        WorkflowDetailsDto? dto = await db.Workflows.AsNoTracking()
            .Where(x => x.Id == request.WorkflowId.Value)
            .Select(w => new WorkflowDetailsDto
            {
                IsActive = w.IsActive,
                Outcome =  w.Outcome,
                Phase = w.Phase,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                Originator = w.InitiatedBy.Name,
                DocumentId = w.DocumentId,
                WorkflowBaseId = w.WorkflowTemplate.WorkflowBaseId
            })
            .FirstOrDefaultAsync(cancellationToken: ct);

        return dto is not null 
            ? Result<WorkflowDetailsDto>.Success(dto) 
            : Result<WorkflowDetailsDto>.Failure(ErrorCode.NotFound);
    }
}