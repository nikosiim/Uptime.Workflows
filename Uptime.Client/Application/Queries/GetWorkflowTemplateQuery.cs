using MediatR;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Application.Services;
using Uptime.Shared.Common;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.Client.Application.Queries;

public record GetWorkflowTemplateQuery(int TemplateId) : IRequest<Result<WorkflowTemplate>>;

public class GetWorkflowTemplateQueryHandler(IApiService apiService) 
    : IRequestHandler<GetWorkflowTemplateQuery, Result<WorkflowTemplate>>
{
    public async Task<Result<WorkflowTemplate>> Handle(GetWorkflowTemplateQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.WorkflowTemplates.GetTemplate.Replace("{templateId}", request.TemplateId.ToString());
        Result<WorkflowTemplateResponse> result = await apiService.ReadFromJsonAsync<WorkflowTemplateResponse>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<WorkflowTemplate>.Failure(result.Error);
        }

        WorkflowTemplateResponse response = result.Value!;

        var template = new WorkflowTemplate
        {
            Id = response.Id,
            Name = response.Name,
            AssociationDataJson = response.AssociationDataJson,
            Created = response.Created,
            WorkflowBaseId = response.WorkflowBaseId
        };
    
        return Result<WorkflowTemplate>.Success(template);
    }
}