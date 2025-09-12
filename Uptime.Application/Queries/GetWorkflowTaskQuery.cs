using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Queries;

public record GetWorkflowTaskQuery(TaskId TaskId) : IRequest<Result<WorkflowTaskDetailsDto>>;

public class GetWorkflowTaskQueryHandler(WorkflowDbContext db)
    : IRequestHandler<GetWorkflowTaskQuery, Result<WorkflowTaskDetailsDto>>
{
    public async Task<Result<WorkflowTaskDetailsDto>> Handle(GetWorkflowTaskQuery request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<WorkflowTaskDetailsDto>.Cancelled();

        WorkflowTaskDetailsDto? dto = await db.WorkflowTasks
            .AsNoTracking()
            .Where(t => t.Id == request.TaskId.Value)
            .Select(task => new WorkflowTaskDetailsDto
            {
                Id = task.Id,
                TaskGuid = task.TaskGuid,
                AssignedTo = (PrincipalId)task.AssignedToPrincipalId,
                AssignedBy = (PrincipalId)task.AssignedByPrincipalId,
                Status = task.Status,
                InternalStatus = task.InternalStatus,
                Description = task.Description,
                DueDate = task.DueDate,
                EndDate = task.EndDate,
                StorageJson = task.StorageJson,
                Document = task.Workflow.Document.Title,
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