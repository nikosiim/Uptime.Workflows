using Fluxor;
using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;
using Uptime.Client.StateManagement.Workflow;

namespace Uptime.Client.Application.Commands;

public record DeleteWorkflowCommand(int WorkflowId) : IRequest<Result<bool>>;

public class DeleteWorkflowCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<DeleteWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        User executor = User.OrSystemAccount(workflowState.Value.CurrentUser);

        var deleteRequest = new WorkflowDeleteRequest
        {
            ExecutorSid = executor.Sid
        };

        string url = ApiRoutes.Workflows.DeleteWorkflow.Replace("{workflowId}", request.WorkflowId.ToString());
        return await apiService.DeleteAsync(url, deleteRequest, cancellationToken);
    }
}