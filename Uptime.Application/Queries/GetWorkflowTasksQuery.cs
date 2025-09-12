using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Application.Queries;

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
                Id = task.Id,
                AssignedTo = task.AssignedTo.Name, // TODO: Decide whether to return just the name or the name with the ID
                AssignedBy = task.AssignedBy.Name, // TODO: Decide whether to return just the name or the name with the ID
                DisplayStatus = task.Status,
                InternalStatus = task.InternalStatus,
                Description = task.Description,
                DueDate = task.DueDate,
                EndDate = task.EndDate,
                StorageJson = task.StorageJson
            })
            .ToListAsync(ct);

        return dtos.Count > 0
            ? Result<List<WorkflowTaskDto>>.Success(dtos)
            : Result<List<WorkflowTaskDto>>.Failure(ErrorCode.NotFound);
    }
}