using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Domain.Common;
using Uptime.Domain.Data;

namespace Uptime.Application.Queries;

public record GetWorkflowTaskQuery(TaskId TaskId) : IRequest<WorkflowTaskDetailsDto?>;

public class GetWorkflowTaskQueryHandler(WorkflowDbContext dbContext) : IRequestHandler<GetWorkflowTaskQuery, WorkflowTaskDetailsDto?>
{
    public Task<WorkflowTaskDetailsDto?> Handle(GetWorkflowTaskQuery request, CancellationToken cancellationToken)
    {
        return dbContext.WorkflowTasks
            .AsNoTracking()
            .Where(task => task.Id == request.TaskId.Value)
            .Select(task => new WorkflowTaskDetailsDto
            {
                Id = task.Id,
                TaskGuid = task.TaskGuid,
                AssignedTo = task.AssignedTo,
                AssignedBy = task.AssignedBy,
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
            .FirstOrDefaultAsync(cancellationToken);
    }

}