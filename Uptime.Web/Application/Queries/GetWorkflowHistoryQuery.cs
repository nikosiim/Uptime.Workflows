using MediatR;
using Uptime.Shared.Models.Workflows;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetWorkflowHistoryQuery(int WorkflowId) : IRequest<List<WorkflowHistoryDto>>;

public class GetWorkflowHistoryQueryHandler(IHttpClientFactory httpClientFactory) 
    : IRequestHandler<GetWorkflowHistoryQuery, List<WorkflowHistoryDto>>
{
    public async Task<List<WorkflowHistoryDto>> Handle(GetWorkflowHistoryQuery request, CancellationToken cancellationToken)
    {
        var result = new List<WorkflowHistoryDto>();
        
        string url = ApiRoutes.Workflows.GetHistories.Replace("{workflowId}", request.WorkflowId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            List<WorkflowHistoryResponse> entries = await response.Content.ReadFromJsonAsync<List<WorkflowHistoryResponse>>(cancellationToken: cancellationToken) ?? [];

            foreach (WorkflowHistoryResponse entry in entries)
            {
                result.Add(new WorkflowHistoryDto
                {
                    Id = entry.Id,
                    WorkflowId = entry.WorkflowId,
                    Description = entry.Description,
                    Occurred = entry.Occurred.ToLocalTime(),
                    Outcome = entry.Outcome,
                    User = entry.User,
                    Event = entry.Event
                });
            }
        }

        return result;
    }
}