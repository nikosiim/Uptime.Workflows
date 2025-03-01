using Fluxor;
using MediatR;
using Uptime.Client.Application.Services;
using Uptime.Client.StateManagement.Workflow;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Commands;

public record CancelWorkflowCommand(int WorkflowId, string Comment) : IRequest<Result<bool>>;

public class CancelWorkflowCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<CancelWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        string executor  = User.GetNameOrSystemAccount(workflowState.Value.CurrentUser);

        var payload = new
        {
            executor, 
            request.Comment
        };

        string url = ApiRoutes.Workflows.CancelWorkflow.Replace("{workflowId}", request.WorkflowId.ToString());
        return await apiService.PostAsJsonAsync(url, payload, cancellationToken);
    }
}