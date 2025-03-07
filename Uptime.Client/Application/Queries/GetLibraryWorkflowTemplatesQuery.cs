using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Application.Services;
using Uptime.Shared.Models.Libraries;

namespace Uptime.Client.Application.Queries;

public record GetLibraryWorkflowTemplatesQuery(int LibraryId) : IRequest<Result<List<WorkflowTemplate>>>;

public class GetLibraryWorkflowTemplatesQueryHandler(IApiService apiService)
    : IRequestHandler<GetLibraryWorkflowTemplatesQuery, Result<List<WorkflowTemplate>>>
{
    public async Task<Result<List<WorkflowTemplate>>> Handle(GetLibraryWorkflowTemplatesQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Libraries.GetWorkflowTemplates.Replace("{libraryId}", request.LibraryId.ToString());
        Result<List<LibraryWorkflowTemplateResponse>> result = await apiService.ReadFromJsonAsync<List<LibraryWorkflowTemplateResponse>>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<List<WorkflowTemplate>>.Failure(result.Error);
        }

        List<WorkflowTemplate> templates = result.Value?.Select(template => new WorkflowTemplate
        {
            Id = template.Id,
            WorkflowBaseId = template.WorkflowBaseId,
            Name = template.Name,
            AssociationDataJson = template.AssociationDataJson,
            Created = template.Created
        }).ToList() ?? [];

        return Result<List<WorkflowTemplate>>.Success(templates);
    }
}