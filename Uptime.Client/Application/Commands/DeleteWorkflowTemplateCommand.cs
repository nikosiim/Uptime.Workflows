using Fluxor;
using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;
using Uptime.Client.StateManagement.Workflow;

namespace Uptime.Client.Application.Commands;

public record DeleteWorkflowTemplateCommand(int TemplateId) : IRequest<Result<bool>>;

public class DeleteWorkflowTemplateCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<DeleteWorkflowTemplateCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        User executor = User.OrSystemAccount(workflowState.Value.CurrentUser);

        var deleteRequest = new WorkflowTemplateDeleteRequest
        {
            ExecutorSid = executor.Sid
        };

        string url = ApiRoutes.WorkflowTemplates.DeleteTemplate.Replace("{templateId}", request.TemplateId.ToString());
        return await apiService.DeleteAsync(url, deleteRequest, cancellationToken);
    }
}