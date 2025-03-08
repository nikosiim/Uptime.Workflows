using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Application.Services;
using Uptime.Shared.Models.Libraries;

namespace Uptime.Client.Application.Queries;

public record GetLibraryDocumentsQuery(string LibraryName) : IRequest<Result<List<LibraryDocument>>>;

public class GetDocumentsQueryHandler(IApiService apiService) 
    : IRequestHandler<GetLibraryDocumentsQuery, Result<List<LibraryDocument>>>
{
    public async Task<Result<List<LibraryDocument>>> Handle(GetLibraryDocumentsQuery request, CancellationToken cancellationToken)
    {
        Constants.Libraries.TryGetValue(request.LibraryName, out int libraryId);

        string url = ApiRoutes.Libraries.GetDocuments.Replace("{libraryId}", libraryId.ToString());
        Result<List<LibraryDocumentResponse>> result = await apiService.ReadFromJsonAsync<List<LibraryDocumentResponse>>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<List<LibraryDocument>>.Failure(result.Error);
        }

        List<LibraryDocument> documents = result.Value?.Select(document => new LibraryDocument
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            LibraryId = libraryId
        }).ToList() ?? [];

        return Result<List<LibraryDocument>>.Success(documents);
    }
}