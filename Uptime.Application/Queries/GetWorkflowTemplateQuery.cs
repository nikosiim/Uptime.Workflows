using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Queries;

public record GetWorkflowTemplateQuery(WorkflowTemplateId TemplateId) : IRequest<Result<WorkflowTemplateDto>>;

public class GetWorkflowTemplateQueryHandler(WorkflowDbContext db) 
    : IRequestHandler<GetWorkflowTemplateQuery, Result<WorkflowTemplateDto>>
{
    public async Task<Result<WorkflowTemplateDto>> Handle(GetWorkflowTemplateQuery request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<WorkflowTemplateDto>.Cancelled();

        WorkflowTemplateDto? dto = await db.WorkflowTemplates.AsNoTracking()
            .Where(wt => wt.Id == request.TemplateId.Value)
            .Select(wt => new WorkflowTemplateDto
            {
                Id = wt.Id,
                Name = wt.TemplateName,
                AssociationDataJson = wt.AssociationDataJson,
                WorkflowBaseId = wt.WorkflowBaseId,
                Created = wt.Created
            })
            .FirstOrDefaultAsync(ct);

        return dto is null 
            ? Result<WorkflowTemplateDto>.Failure(ErrorCode.NotFound) 
            : Result<WorkflowTemplateDto>.Success(dto);
    }
}