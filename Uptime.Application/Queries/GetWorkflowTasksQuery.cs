using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Queries;

public record GetWorkflowTasksQuery(int WorkflowId) : IRequest<List<WorkflowTaskDto>>;

public class GetWorkflowTasksQueryHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<GetWorkflowTasksQuery, List<WorkflowTaskDto>>
{
    public async Task<List<WorkflowTaskDto>> Handle(GetWorkflowTasksQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowTasks
            .Where(task => task.WorkflowId == request.WorkflowId)
            .Select(task => new WorkflowTaskDto
            {
                TaskId = task.Id,
                WorkflowId = task.WorkflowId,
                AssignedTo = task.AssignedTo,
                Status = task.Status,
                TaskDescription = task.TaskDescription,
                DueDate = task.DueDate,
                EndDate = task.EndDate
            })
            .ToListAsync(cancellationToken);
    }
}