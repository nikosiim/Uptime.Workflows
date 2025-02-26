using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;
using Uptime.Shared.Extensions;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.Application.Queries;

public record GetWorkflowDetailsQuery(int WorkflowId) : IRequest<Result<WorkflowDetails>>;

public class GetWorkflowDetailsQueryHandler(IApiService apiService)
    : IRequestHandler<GetWorkflowDetailsQuery, Result<WorkflowDetails>>
{
    public async Task<Result<WorkflowDetails>> Handle(GetWorkflowDetailsQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Workflows.GetWorkflow.Replace("{workflowId}", request.WorkflowId.ToString());
        Result<WorkflowResponse> result = await apiService.ReadFromJsonAsync<WorkflowResponse>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<WorkflowDetails>.Failure(result.Error);
        }

        WorkflowResponse details = result.Value!;

        var template = new WorkflowDetails
        {
            DocumentId = details.DocumentId,
            Document = details.Document,
            Originator = details.Originator,
            StartDate = details.StartDate,
            EndDate = details.EndDate,
            Outcome = WorkflowResources.Get(details.Outcome),
            IsActive = details.IsActive
        };
    
        return Result<WorkflowDetails>.Success(template);
    }
}