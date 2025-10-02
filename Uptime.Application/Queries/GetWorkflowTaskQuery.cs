using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Queries;

public record GetWorkflowTaskQuery(Guid TaskGuid) : IRequest<Result<WorkflowTaskDetailsDto>>;

public class GetWorkflowTaskQueryHandler(WorkflowDbContext db)
    : IRequestHandler<GetWorkflowTaskQuery, Result<WorkflowTaskDetailsDto>>
{
    public async Task<Result<WorkflowTaskDetailsDto>> Handle(GetWorkflowTaskQuery request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<WorkflowTaskDetailsDto>.Cancelled();

        WorkflowTaskDetailsDto? dto = await db.WorkflowTasks
            .AsNoTracking()
            .Where(t => t.TaskGuid == request.TaskGuid)
            .Select(task => new WorkflowTaskDetailsDto
            {
                TaskGuid = task.TaskGuid,
                AssignedTo = task.AssignedTo.Name,
                AssignedBy = task.AssignedBy.Name,
                Status = task.InternalStatus.ToString(),
                InternalStatus = task.InternalStatus,
                Description = task.Description,
                DueDate = task.DueDate,
                EndDate = task.EndDate,
                StorageJson = task.StorageJson,
                DocumentId = (DocumentId)task.Workflow.DocumentId,
                WorkflowId = task.WorkflowId,
                PhaseId = task.PhaseId,
                WorkflowBaseId = task.Workflow.WorkflowTemplate.WorkflowBaseId
            })
            .FirstOrDefaultAsync(ct);

        return dto is not null 
            ? Result<WorkflowTaskDetailsDto>.Success(dto) 
            : Result<WorkflowTaskDetailsDto>.Failure(ErrorCode.NotFound);
    }
}