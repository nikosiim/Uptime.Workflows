using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;
using Uptime.Shared.Extensions;
using Uptime.Shared.Models.Tasks;

namespace Uptime.Client.Application.Queries;

public record GetWorkflowTaskQuery(int TaskId) : IRequest<Result<WorkflowTaskDetails>>;

public class GetWorkflowTaskQueryHandler(IApiService apiService) 
    : IRequestHandler<GetWorkflowTaskQuery, Result<WorkflowTaskDetails>>
{
    public async Task<Result<WorkflowTaskDetails>> Handle(GetWorkflowTaskQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Tasks.GetTask.Replace("{taskId}", request.TaskId.ToString());
        Result<WorkflowTaskResponse> result = await apiService.ReadFromJsonAsync<WorkflowTaskResponse>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<WorkflowTaskDetails>.Failure(result.Error);
        }

        WorkflowTaskResponse task = result.Value;

        var template = new WorkflowTaskDetails
        {
            Id = task.Id,
            AssignedTo = task.AssignedTo,
            AssignedBy = task.AssignedBy,
            Description = task.Description,
            DueDate = task.DueDate,
            EndDate = task.EndDate,
            Status = WorkflowTaskResources.Get(task.Status),
            InternalStatus = task.InternalStatus,
            StorageJson = task.StorageJson,
            Document = task.Document,
            WorkflowId = task.WorkflowId
        };
    
        return Result<WorkflowTaskDetails>.Success(template);
    }
}