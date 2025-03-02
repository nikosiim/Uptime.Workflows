using MediatR;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Application.Services;
using Uptime.Shared.Common;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.Application.Queries;

public record GetModificationContextQuery(int WorkflowId) : IRequest<Result<ModificationContext>>;

public class GetModificationContextQueryHandler(IApiService apiService) 
    : IRequestHandler<GetModificationContextQuery, Result<ModificationContext>>
{
    public async Task<Result<ModificationContext>> Handle(GetModificationContextQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Workflows.GetModificationContext.Replace("{workflowId}", request.WorkflowId.ToString());
        Result<ModificationContextResponse> result = await apiService.ReadFromJsonAsync<ModificationContextResponse>(url, cancellationToken);

        if (!result.Succeeded)
        {
            return Result<ModificationContext>.Failure(result.Error);
        }

        ModificationContextResponse response = result.Value!;

        var data = new ModificationContext
        {
            WorkflowId = response.WorkflowId,
            PhaseId = response.PhaseId,
            ContextTasks = response.ContextTasks?
                .Select(t => new ContextTask
                {
                    AssignedTo = t.AssignedTo,
                    TaskGuid = t.TaskGuid
                }).ToList()
        };
    
        return Result<ModificationContext>.Success(data);
    }
}