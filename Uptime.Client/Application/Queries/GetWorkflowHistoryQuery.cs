using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.Application.Queries;

public record GetWorkflowHistoryQuery(int WorkflowId) : IRequest<Result<List<WorkflowHistory>>>;

public class GetWorkflowHistoryQueryHandler(IApiService apiService) 
    : IRequestHandler<GetWorkflowHistoryQuery, Result<List<WorkflowHistory>>>
{
    public async Task<Result<List<WorkflowHistory>>> Handle(GetWorkflowHistoryQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Workflows.GetHistories.Replace("{workflowId}", request.WorkflowId.ToString());
        Result<List<WorkflowHistoryResponse>> result = await apiService.ReadFromJsonAsync<List<WorkflowHistoryResponse>>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<List<WorkflowHistory>>.Failure(result.Error);
        }

        List<WorkflowHistory> histories = result.Value?.Select(entry
            => new WorkflowHistory
            {
                Id = entry.Id,
                WorkflowId = entry.WorkflowId,
                Description = entry.Description,
                Occurred = entry.Occurred.ToLocalTime(),
                Comment = entry.Comment,
                User = entry.User,
                Event = entry.Event
            }).ToList() ?? [];

        return Result<List<WorkflowHistory>>.Success(histories);
    }
}