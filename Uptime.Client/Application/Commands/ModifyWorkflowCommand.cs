using Fluxor;
using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.StateManagement.Workflow;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.Application.Commands;

public record ModifyWorkflowCommand(int WorkflowId, string ModificationContext) : IRequest<Result<bool>>;

public class ModifyWorkflowCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<ModifyWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ModifyWorkflowCommand request, CancellationToken cancellationToken)
    {
        string executorSid  = User.GetNameOrSystemAccount(workflowState.Value.CurrentUser);

        var payload = new ModifyWorkflowRequest(executorSid, request.ModificationContext);

        string url = ApiRoutes.Workflows.ModifyWorkflow .Replace("{workflowId}", request.WorkflowId.ToString());
        return await apiService.PostAsJsonAsync(url, payload, cancellationToken);
    }
}