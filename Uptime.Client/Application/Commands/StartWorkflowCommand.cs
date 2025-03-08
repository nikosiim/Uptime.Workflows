using Fluxor;
using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.StateManagement.Workflow;

namespace Uptime.Client.Application.Commands;

public record StartWorkflowCommand : IRequest<Result<bool>>
{
    public required int DocumentId { get; set; }
    public required int WorkflowTemplateId { get; set; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class StartWorkflowCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<StartWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        string originator = User.GetNameOrSystemAccount(workflowState.Value.CurrentUser);

        var payload = new
        {
            originator, 
            request.DocumentId, 
            request.WorkflowTemplateId, 
            request.Storage
        };

        return await apiService.PostAsJsonAsync(ApiRoutes.Workflows.StartWorkflow, payload, cancellationToken);
    }
}