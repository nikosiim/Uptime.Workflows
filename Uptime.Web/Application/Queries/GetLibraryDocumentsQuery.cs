using MediatR;
using Uptime.Shared.Models.Libraries;
using Uptime.Web.Application.DTOs;

namespace Uptime.Web.Application.Queries;

public record GetLibraryDocumentsQuery(string LibraryName) : IRequest<List<LibraryDocument>>;

public class GetDocumentsQueryHandler(IHttpClientFactory httpClientFactory) 
    : IRequestHandler<GetLibraryDocumentsQuery, List<LibraryDocument>>
{
    public async Task<List<LibraryDocument>> Handle(GetLibraryDocumentsQuery request, CancellationToken cancellationToken)
    {
        var result = new List<LibraryDocument>();

        Constants.Libraries.TryGetValue(request.LibraryName, out int libraryId);

        string url = ApiRoutes.Libraries.GetDocuments.Replace("{libraryId}", libraryId.ToString());

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch documents. Status code: {response.StatusCode}");
        }

        List<LibraryDocumentResponse> documents = await response.Content.ReadFromJsonAsync<List<LibraryDocumentResponse>>(cancellationToken: cancellationToken) ?? [];

        foreach (LibraryDocumentResponse document in documents)
        {
            result.Add(new LibraryDocument
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                LibraryId = libraryId
            });
        }

        return result;
    }
}