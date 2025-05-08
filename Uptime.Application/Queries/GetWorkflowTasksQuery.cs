using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Application.Queries;

public record GetWorkflowTasksQuery(WorkflowId WorkflowId, WorkflowTaskStatus? Status) : IRequest<List<WorkflowTaskDto>>;

public class GetWorkflowTasksQueryHandler(WorkflowDbContext dbContext)
    : IRequestHandler<GetWorkflowTasksQuery, List<WorkflowTaskDto>>
{
    public async Task<List<WorkflowTaskDto>> Handle(GetWorkflowTasksQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowTasks.AsNoTracking()
            .Where(task => task.WorkflowId == request.WorkflowId.Value && (request.Status == null || task.InternalStatus == request.Status))
            .Select(task => new WorkflowTaskDto
            {
                Id = task.Id,
                AssignedTo = task.AssignedTo,
                AssignedBy = task.AssignedBy,
                DisplayStatus = task.Status,
                InternalStatus = task.InternalStatus,
                Description = task.Description,
                DueDate = task.DueDate,
                EndDate = task.EndDate,
                StorageJson = task.StorageJson
            })
            .ToListAsync(cancellationToken);
    }
}