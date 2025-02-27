using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Commands;

public record CancelWorkflowCommand(int WorkflowId, string Executor, string Comment) : IRequest<Result<bool>>;

public class CancelWorkflowCommandHandler(IApiService apiService)
    : IRequestHandler<CancelWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        var payload = new
        {
            request.Executor, 
            request.WorkflowId, 
            request.Comment
        };

        string url = ApiRoutes.Workflows.CancelWorkflow.Replace("{workflowId}", request.WorkflowId.ToString());
        return await apiService.PostAsJsonAsync(url, payload, cancellationToken);
    }
}