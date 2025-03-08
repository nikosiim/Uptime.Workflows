using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;

namespace Uptime.Client.Application.Commands;

public record DeleteWorkflowCommand(int WorkflowId) : IRequest<Result<bool>>;

public class DeleteWorkflowCommandHandler(IApiService apiService)
    : IRequestHandler<DeleteWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Workflows.DeleteWorkflow.Replace("{workflowId}", request.WorkflowId.ToString());
        return await apiService.DeleteAsync(url, cancellationToken);
    }
}