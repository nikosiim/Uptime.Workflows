using MediatR;
using Uptime.Shared.Enums;
using Uptime.Shared.Models.Workflows;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetWorkflowTasksQuery(int WorkflowId, WorkflowTaskStatus? Status = null) : IRequest<List<WorkflowTaskDto>>;

public class GetWorkflowTasksQueryHandler(IHttpClientFactory httpClientFactory) 
    : IRequestHandler<GetWorkflowTasksQuery, List<WorkflowTaskDto>>
{
    public async Task<List<WorkflowTaskDto>> Handle(GetWorkflowTasksQuery request, CancellationToken cancellationToken)
    {
        var result = new List<WorkflowTaskDto>();
        
        string url = ApiRoutes.Workflows.GetTasks.Replace("{workflowId}", request.WorkflowId.ToString());

        if (request.Status is not null)
        {
            url += $"?status={request.Status.ToString()}";
        }

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            List<WorkflowTasksResponse> tasks = await response.Content.ReadFromJsonAsync<List<WorkflowTasksResponse>>(cancellationToken: cancellationToken) ?? [];

            foreach (WorkflowTasksResponse task in tasks)
            {
                result.Add(new WorkflowTaskDto
                {
                    Id = task.Id,
                    AssignedTo = task.AssignedTo,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    EndDate = task.EndDate,
                    Status = task.Status,
                    InternalStatus = task.InternalStatus,
                    StorageJson = task.StorageJson,
                    WorkflowId = request.WorkflowId
                });
            }
        }

        return result;
    }
}