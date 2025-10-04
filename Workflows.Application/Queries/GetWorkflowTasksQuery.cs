using Microsoft.EntityFrameworkCore;
using Workflows.Application.DTOs;
using Workflows.Application.Messaging;
using Workflows.Core.Common;
using Workflows.Core.Data;
using Workflows.Core.Enums;

namespace Workflows.Application.Queries;

public record GetWorkflowTasksQuery(WorkflowId WorkflowId, WorkflowTaskStatus? Status) : IRequest<Result<List<WorkflowTaskDto>>>;

public class GetWorkflowTasksQueryHandler(WorkflowDbContext db)
    : IRequestHandler<GetWorkflowTasksQuery, Result<List<WorkflowTaskDto>>>
{
    public async Task<Result<List<WorkflowTaskDto>>> Handle(GetWorkflowTasksQuery request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<List<WorkflowTaskDto>>.Cancelled();
        
        List<WorkflowTaskDto> dtos = await db.WorkflowTasks.AsNoTracking()
            .Where(task => task.WorkflowId == request.WorkflowId.Value && (request.Status == null || task.InternalStatus == request.Status))
            .Select(task => new WorkflowTaskDto
            {
                TaskGuid = task.TaskGuid,
                AssignedTo = task.AssignedTo.Name,
                AssignedBy = task.AssignedBy.Name,
                DisplayStatus = task.InternalStatus.ToString(),
                InternalStatus = task.InternalStatus,
                Description = task.Description,
                DueDate = task.DueDate,
                EndDate = task.EndDate,
                StorageJson = task.StorageJson
            })
            .ToListAsync(ct);

        return Result<List<WorkflowTaskDto>>.Success(dtos);
    }
}