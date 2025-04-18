using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Application.Services;
using Uptime.Shared.Models.Documents;

namespace Uptime.Client.Application.Queries;

public record GetDocumentWorkflowsQuery(int DocumentId) : IRequest<Result<List<DocumentWorkflow>>>;

public class GetDocumentWorkflowsQueryHandler(IApiService apiService)
    : IRequestHandler<GetDocumentWorkflowsQuery,Result<List<DocumentWorkflow>>>
{
    public async Task<Result<List<DocumentWorkflow>>> Handle(GetDocumentWorkflowsQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Documents.GetWorkflows.Replace("{documentId}", request.DocumentId.ToString());
        Result<List<DocumentWorkflowsResponse>> result = await apiService.ReadFromJsonAsync<List<DocumentWorkflowsResponse>>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<List<DocumentWorkflow>>.Failure(result.Error);
        }

        List<DocumentWorkflow> workflows = result.Value?.Select(workflow => new DocumentWorkflow
        {
            Id = workflow.Id,
            TemplateId = workflow.TemplateId,
            WorkflowTemplateName = workflow.WorkflowTemplateName,
            StartDate = workflow.StartDate,
            EndDate = workflow.EndDate,
            Outcome = WorkflowResources.Get(workflow.Outcome),
            IsActive = workflow.IsActive
        }).ToList() ?? [];

        return Result<List<DocumentWorkflow>>.Success(workflows);
    }
}