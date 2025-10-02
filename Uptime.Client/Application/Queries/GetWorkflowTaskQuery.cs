using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;

namespace Uptime.Client.Application.Queries;

public record GetWorkflowTaskQuery(Guid TaskGuid) : IRequest<Result<WorkflowTaskDetails>>;

public class GetWorkflowTaskQueryHandler(IApiService apiService) 
    : IRequestHandler<GetWorkflowTaskQuery, Result<WorkflowTaskDetails>>
{
    public async Task<Result<WorkflowTaskDetails>> Handle(GetWorkflowTaskQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Tasks.GetTask.Replace("{taskGuid}", request.TaskGuid.ToString());
        Result<WorkflowTaskResponse> result = await apiService.ReadFromJsonAsync<WorkflowTaskResponse>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<WorkflowTaskDetails>.Failure(result.Error);
        }

        WorkflowTaskResponse task = result.Value;

        var template = new WorkflowTaskDetails
        {
            TaskGuid = task.TaskGuid,
            AssignedTo = task.AssignedTo,
            AssignedBy = task.AssignedBy,
            Description = task.Description,
            DueDate = task.DueDate,
            EndDate = task.EndDate,
            Status = WorkflowTaskResources.Get(task.Status),
            InternalStatus = (WorkflowTaskStatus)task.InternalStatus,
            StorageJson = task.StorageJson,
            DocumentId = task.DocumentId,
            Document = "Document Y",
            WorkflowId = task.WorkflowId,
            PhaseId = task.PhaseId,
            WorkflowBaseId = task.WorkflowBaseId
        };
    
        return Result<WorkflowTaskDetails>.Success(template);
    }
}