using MediatR;
using Uptime.Shared.Models.Libraries;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetLibraryWorkflowTemplatesQuery(int LibraryId) : IRequest<List<WorkflowTemplate>>;

public class GetLibraryWorkflowTemplatesQueryHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<GetLibraryWorkflowTemplatesQuery, List<WorkflowTemplate>>
{
    public async Task<List<WorkflowTemplate>> Handle(GetLibraryWorkflowTemplatesQuery request, CancellationToken cancellationToken)
    {
        var result = new List<WorkflowTemplate>();

        string url = ApiRoutes.Libraries.GetWorkflowTemplates.Replace("{libraryId}", request.LibraryId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            List<LibraryWorkflowTemplateResponse> workflowTemplates =
                await response.Content.ReadFromJsonAsync<List<LibraryWorkflowTemplateResponse>>(cancellationToken: cancellationToken) ?? [];

            foreach (LibraryWorkflowTemplateResponse template in workflowTemplates)
            {
                result.Add(new WorkflowTemplate
                {
                    Id = template.Id,
                    WorkflowBaseId = template.WorkflowBaseId,
                    Name = template.Name,
                    AssociationDataJson = template.AssociationDataJson,
                    Created = template.Created
                });
            }
        }

        return result;
    }
}