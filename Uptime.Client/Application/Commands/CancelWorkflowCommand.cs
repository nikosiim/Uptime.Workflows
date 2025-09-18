using Fluxor;
using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;
using Uptime.Client.StateManagement.Workflow;

namespace Uptime.Client.Application.Commands;

public record CancelWorkflowCommand(int WorkflowId, string Comment) : IRequest<Result<bool>>;

public class CancelWorkflowCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<CancelWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        User executor = User.OrSystemAccount(workflowState.Value.CurrentUser);

        var payload = new CancelWorkflowRequest (executor.Sid, request.Comment);

        string url = ApiRoutes.Workflows.CancelWorkflow.Replace("{workflowId}", request.WorkflowId.ToString());
        return await apiService.PostAsJsonAsync(url, payload, cancellationToken);
    }
}