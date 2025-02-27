using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.Application.Queries;

public record GetWorkflowHistoryQuery(int WorkflowId) : IRequest<Result<List<WorkflowHistoryData>>>;

public class GetWorkflowHistoryQueryHandler(IApiService apiService) 
    : IRequestHandler<GetWorkflowHistoryQuery, Result<List<WorkflowHistoryData>>>
{
    public async Task<Result<List<WorkflowHistoryData>>> Handle(GetWorkflowHistoryQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Workflows.GetHistories.Replace("{workflowId}", request.WorkflowId.ToString());
        Result<List<WorkflowHistoryResponse>> result = await apiService.ReadFromJsonAsync<List<WorkflowHistoryResponse>>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<List<WorkflowHistoryData>>.Failure(result.Error);
        }

        List<WorkflowHistoryData> histories = result.Value?.Select(entry
            => new WorkflowHistoryData
            {
                Id = entry.Id,
                WorkflowId = entry.WorkflowId,
                Description = entry.Description,
                Occurred = entry.Occurred.ToLocalTime(),
                Comment = entry.Comment,
                User = entry.User,
                Event = entry.Event
            }).ToList() ?? [];

        return Result<List<WorkflowHistoryData>>.Success(histories);
    }
}