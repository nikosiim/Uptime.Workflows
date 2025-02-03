using MediatR;
using Uptime.Shared.Models.Documents;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetDocumentWorkflowsQuery(int DocumentId) : IRequest<List<DocumentWorkflowDto>>;

public class GetDocumentWorkflowsQueryHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<GetDocumentWorkflowsQuery, List<DocumentWorkflowDto>>
{
    public async Task<List<DocumentWorkflowDto>> Handle(GetDocumentWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var result = new List<DocumentWorkflowDto>();

        string url = ApiRoutes.Documents.GetWorkflows.Replace("{documentId}", request.DocumentId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            List<DocumentWorkflowsResponse> workflows =
                await response.Content.ReadFromJsonAsync<List<DocumentWorkflowsResponse>>(cancellationToken: cancellationToken) ?? [];

            foreach (DocumentWorkflowsResponse workflow in workflows)
            {
                result.Add(new DocumentWorkflowDto
                {
                    Id = workflow.Id,
                    TemplateId = workflow.TemplateId,
                    WorkflowTemplateName = workflow.WorkflowTemplateName,
                    StartDate = workflow.StartDate,
                    EndDate = workflow.EndDate,
                    Status = workflow.Status
                });
            }
        }

        return result;
    }
}