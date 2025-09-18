using Fluxor;
using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;
using Uptime.Client.StateManagement.Workflow;

namespace Uptime.Client.Application.Commands;

public record StartWorkflowCommand : IRequest<Result<bool>>
{
    public required int DocumentId { get; init; }
    public required int WorkflowTemplateId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class StartWorkflowCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<StartWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        User originator = User.OrSystemAccount(workflowState.Value.CurrentUser);

        var payload = new StartWorkflowRequest(originator.Sid, request.DocumentId, request.WorkflowTemplateId)
        {
            Storage = request.Storage
        };

        return await apiService.PostAsJsonAsync(ApiRoutes.Workflows.StartWorkflow, payload, cancellationToken);
    }
}