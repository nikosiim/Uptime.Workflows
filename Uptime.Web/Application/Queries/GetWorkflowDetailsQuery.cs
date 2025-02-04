using MediatR;
using Uptime.Shared.Models.Workflows;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetWorkflowDetailsQuery(int WorkflowId) : IRequest<WorkflowDetailsDto>;

public class GetWorkflowDetailsQueryHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<GetWorkflowDetailsQuery, WorkflowDetailsDto>
{
    public async Task<WorkflowDetailsDto> Handle(GetWorkflowDetailsQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Workflows.GetWorkflow.Replace("{workflowId}", request.WorkflowId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        WorkflowResponse details = await response.Content.ReadFromJsonAsync<WorkflowResponse>(cancellationToken: cancellationToken)
            ?? throw new HttpRequestException($"Failed to deserialize workflow details. Status code: {response.StatusCode}");
        
        return new WorkflowDetailsDto
        {
            DocumentId = details.DocumentId,
            Document = details.Document,
            Originator = details.Originator,
            StartDate = details.StartDate,
            EndDate = details.EndDate,
            Status = details.Status
        };
    }
}