using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;
using Uptime.Shared.Enums;
using Uptime.Shared.Extensions;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.Application.Queries;

public record GetWorkflowTasksQuery(int WorkflowId, WorkflowTaskStatus? Status = null) : IRequest<Result<List<WorkflowTaskData>>>;

public class GetWorkflowTasksQueryHandler(IApiService apiService) 
    : IRequestHandler<GetWorkflowTasksQuery, Result<List<WorkflowTaskData>>>
{
    public async Task<Result<List<WorkflowTaskData>>> Handle(GetWorkflowTasksQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Workflows.GetTasks.Replace("{workflowId}", request.WorkflowId.ToString());

        if (request.Status is not null)
        {
            url += $"?status={request.Status.ToString()}";
        }

        Result<List<WorkflowTasksResponse>> result = await apiService.ReadFromJsonAsync<List<WorkflowTasksResponse>>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<List<WorkflowTaskData>>.Failure(result.Error);
        }

        List<WorkflowTaskData> workflowTasks = result.Value?.Select(task => new WorkflowTaskData
        {
            Id = task.Id,
            AssignedTo = task.AssignedTo,
            Description = task.Description,
            DueDate = task.DueDate,
            EndDate = task.EndDate,
            Status = WorkflowTaskResources.Get(task.Status),
            InternalStatus = task.InternalStatus,
            StorageJson = task.StorageJson,
            WorkflowId = request.WorkflowId
        }).ToList() ?? [];

        return Result<List<WorkflowTaskData>>.Success(workflowTasks);
    }
}