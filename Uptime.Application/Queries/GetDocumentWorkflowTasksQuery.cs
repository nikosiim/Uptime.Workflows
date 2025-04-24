using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Entities;

namespace Uptime.Application.Queries;

public record GetDocumentWorkflowTasksQuery(DocumentId DocumentId) : IRequest<List<DocumentWorkflowTaskDto>>;

public class GetDocumentWorkflowTasksQueryHandler(WorkflowDbContext dbContext)
    : IRequestHandler<GetDocumentWorkflowTasksQuery, List<DocumentWorkflowTaskDto>>
{
    public async Task<List<DocumentWorkflowTaskDto>> Handle(GetDocumentWorkflowTasksQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows.AsNoTracking()
            .Where(wi => wi.DocumentId == request.DocumentId.Value)
            .SelectMany(wi => wi.WorkflowTasks ?? new List<WorkflowTask>())
            .Select(task => new DocumentWorkflowTaskDto
            {
                TaskId = task.Id,
                WorkflowId = task.WorkflowId,
                AssignedTo = task.AssignedTo,
                Status = task.Status,
                WorkflowTaskStatus = task.InternalStatus,
                TaskDescription = task.Description,
                DueDate = task.DueDate,
                EndDate = task.EndDate
            })
            .ToListAsync(cancellationToken);
    }
}