using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Queries;

public record GetWorkflowTasksQuery(WorkflowId WorkflowId, WorkflowTaskStatus? Status) : IRequest<List<WorkflowTaskDto>>;

public class GetWorkflowTasksQueryHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<GetWorkflowTasksQuery, List<WorkflowTaskDto>>
{
    public async Task<List<WorkflowTaskDto>> Handle(GetWorkflowTasksQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowTasks
            .Where(task => task.WorkflowId == request.WorkflowId.Value && (request.Status == null || task.InternalStatus == request.Status))
            .Select(task => new WorkflowTaskDto
            {
                Id = task.Id,
                AssignedTo = task.AssignedTo,
                AssignedBy = task.AssignedBy,
                Status = task.Status,
                InternalStatus = task.InternalStatus,
                Description = task.Description,
                DueDate = task.DueDate,
                EndDate = task.EndDate,
                StorageJson = task.StorageJson
            })
            .ToListAsync(cancellationToken);
    }
}