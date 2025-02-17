using MediatR;
using Uptime.Shared.Models.Tasks;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetWorkflowTaskQuery(int TaskId) : IRequest<WorkflowTaskDto?>;

public class GetWorkflowTaskQueryHandler(IHttpClientFactory httpClientFactory) 
    : IRequestHandler<GetWorkflowTaskQuery, WorkflowTaskDto?>
{
    public async Task<WorkflowTaskDto?> Handle(GetWorkflowTaskQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Tasks.GetTask.Replace("{taskId}", request.TaskId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
        
        response.EnsureSuccessStatusCode();

        WorkflowTaskResponse task = await response.Content.ReadFromJsonAsync<WorkflowTaskResponse>(cancellationToken: cancellationToken)
            ?? throw new HttpRequestException($"Failed to fetch task details. Status code: {response.StatusCode}");

        return new WorkflowTaskDto
        {
            Id = task.Id,
            AssignedTo = task.AssignedTo,
            AssignedBy = task.AssignedBy,
            Description = task.Description,
            DueDate = task.DueDate,
            EndDate = task.EndDate,
            Status = task.Status,
            InternalStatus = task.InternalStatus,
            StorageJson = task.StorageJson,
            Document = task.Document,
            WorkflowId = task.WorkflowId
        };
    }
}