using MediatR;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.Web.Application.Queries;

public record GetWorkflowTemplateQuery(int TemplateId) : IRequest<WorkflowTemplateResponse?>;

public class GetWorkflowTemplateQueryHandler(IHttpClientFactory httpClientFactory) 
    : IRequestHandler<GetWorkflowTemplateQuery, WorkflowTemplateResponse?>
{
    public async Task<WorkflowTemplateResponse?> Handle(GetWorkflowTemplateQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.WorkflowTemplates.GetTemplate.Replace("{templateId}", request.TemplateId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch the template. Status code: {response.StatusCode}");
        }

        return await response.Content.ReadFromJsonAsync<WorkflowTemplateResponse>(cancellationToken: cancellationToken);
    }
}