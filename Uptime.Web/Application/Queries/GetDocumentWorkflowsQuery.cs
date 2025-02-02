using MediatR;
using Uptime.Shared.Models.Documents;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetDocumentWorkflowsQuery(int DocumentId) : IRequest<List<DocumentWorkflow>>;

public class GetDocumentWorkflowsQueryHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<GetDocumentWorkflowsQuery, List<DocumentWorkflow>>
{
    public async Task<List<DocumentWorkflow>> Handle(GetDocumentWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var result = new List<DocumentWorkflow>();

        string url = ApiRoutes.Documents.GetWorkflows.Replace("{documentId}", request.DocumentId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            List<DocumentWorkflowsResponse> workflows = await response.Content.ReadFromJsonAsync<List<DocumentWorkflowsResponse>>(cancellationToken: cancellationToken) ?? [];

            foreach (DocumentWorkflowsResponse workflow in workflows)
            {
                result.Add(new DocumentWorkflow
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