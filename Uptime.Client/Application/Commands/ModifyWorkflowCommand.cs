using Fluxor;
using MediatR;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Application.Services;
using Uptime.Client.StateManagement.Workflow;
using Uptime.Shared.Common;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.Application.Commands;

public record ModifyWorkflowCommand(ModificationContext ModificationContext) : IRequest<Result<bool>>;

public class ModifyWorkflowCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<ModifyWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ModifyWorkflowCommand request, CancellationToken cancellationToken)
    {
        string executor  = User.GetNameOrSystemAccount(workflowState.Value.CurrentUser);

        var payload = new ModifyWorkflowRequest
        {
            Executor = executor,
            WorkflowId = request.ModificationContext.WorkflowId,
            PhaseId = request.ModificationContext.PhaseId,
            ContextTasks = request.ModificationContext.ContextTasks?
                .Select(task => new ContextTaskRequest
                {
                    AssignedTo = task.AssignedTo,
                    TaskGuid = task.TaskGuid
                })
                .ToList()
        };

        string url = ApiRoutes.Workflows.ModifyWorkflow .Replace("{workflowId}", request.ModificationContext.WorkflowId.ToString());
        return await apiService.PostAsJsonAsync(url, payload, cancellationToken);
    }
}