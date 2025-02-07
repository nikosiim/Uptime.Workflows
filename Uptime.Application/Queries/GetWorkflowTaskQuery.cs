using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Queries;

public record GetWorkflowTaskQuery(int TaskId) : IRequest<WorkflowTaskDto?>;

public class GetWorkflowTaskQueryHandler(IWorkflowDbContext dbContext) : IRequestHandler<GetWorkflowTaskQuery, WorkflowTaskDto?>
{
    public async Task<WorkflowTaskDto?> Handle(GetWorkflowTaskQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowTasks
            .Where(task => task.Id == request.TaskId)
            .Select(task => new WorkflowTaskDto
            {
                Id = task.Id,
                AssignedTo = task.AssignedTo,
                AssignedBy = task.AssignedBy,
                Status = task.Status,
                Description = task.TaskDescription,
                DueDate = task.DueDate,
                EndDate = task.EndDate,
                StorageJson = task.StorageJson,
                Document = task.Workflow.Document.Title,
                WorkflowId = task.WorkflowId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}