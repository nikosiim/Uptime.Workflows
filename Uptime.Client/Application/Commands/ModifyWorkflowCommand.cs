using Fluxor;
using MediatR;
using Uptime.Client.Application.Services;
using Uptime.Client.StateManagement.Workflow;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Commands;

public record ModifyWorkflowCommand(int WorkflowId, IEnumerable<string> AssignedTo) : IRequest<Result<bool>>;

public class ModifyWorkflowCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<ModifyWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ModifyWorkflowCommand request, CancellationToken cancellationToken)
    {
        string executor  = User.GetNameOrSystemAccount(workflowState.Value.CurrentUser);

        var payload = new
        {
            executor, 
            request.AssignedTo
        };

        string url = ApiRoutes.Workflows.ModifyWorkflow .Replace("{workflowId}", request.WorkflowId.ToString());
        return await apiService.PostAsJsonAsync(url, payload, cancellationToken);
    }
}