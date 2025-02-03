using MediatR;
using Uptime.Shared.Models.WorkflowTemplates;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetWorkflowTemplateQuery(int TemplateId) : IRequest<WorkflowTemplateDto>;

public class GetWorkflowTemplateQueryHandler(IHttpClientFactory httpClientFactory) 
    : IRequestHandler<GetWorkflowTemplateQuery, WorkflowTemplateDto>
{
    public async Task<WorkflowTemplateDto> Handle(GetWorkflowTemplateQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.WorkflowTemplates.GetTemplate.Replace("{templateId}", request.TemplateId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        WorkflowTemplateResponse template = await response.Content.ReadFromJsonAsync<WorkflowTemplateResponse>(cancellationToken: cancellationToken)
                                            ?? throw new HttpRequestException($"Failed to fetch the template. Status code: {response.StatusCode}");

        return new WorkflowTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            AssociationDataJson = template.AssociationDataJson,
            Created = template.Created,
            WorkflowBaseId = template.WorkflowBaseId
        };
    }
}