using Microsoft.EntityFrameworkCore;
using Workflows.Application.DTOs;
using Workflows.Application.Messaging;
using Workflows.Core.Common;
using Workflows.Core.Data;

namespace Workflows.Application.Queries;

public record GetDocumentWorkflowTasksQuery(DocumentId DocumentId) : IRequest<Result<List<DocumentWorkflowTaskDto>>>;

public class GetDocumentWorkflowTasksQueryHandler(WorkflowDbContext db)
    : IRequestHandler<GetDocumentWorkflowTasksQuery, Result<List<DocumentWorkflowTaskDto>>>
{
    public async Task<Result<List<DocumentWorkflowTaskDto>>> Handle(GetDocumentWorkflowTasksQuery request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<List<DocumentWorkflowTaskDto>>.Cancelled();

        List<DocumentWorkflowTaskDto> dtos = await db.Workflows
            .AsNoTracking()
            .Where(wi => wi.DocumentId == request.DocumentId.Value)
            .SelectMany(wi => wi.WorkflowTasks!) 
            .Select(task => new DocumentWorkflowTaskDto
            {
                TaskId = task.Id,
                WorkflowId = task.WorkflowId,
                AssignedTo = task.AssignedTo.Name, // Not needed include as .Select over navigation properties will handle it.
                Status = task.InternalStatus.ToString(),
                WorkflowTaskStatus = task.InternalStatus,
                TaskDescription = task.Description,
                DueDate = task.DueDate,
                EndDate = task.EndDate
            })
            .ToListAsync(ct);

        return dtos.Count > 0
            ? Result<List<DocumentWorkflowTaskDto>>.Success(dtos)
            : Result<List<DocumentWorkflowTaskDto>>.Failure(ErrorCode.NotFound);
    }
}