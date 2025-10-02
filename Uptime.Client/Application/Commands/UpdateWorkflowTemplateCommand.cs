using Fluxor;
using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;
using Uptime.Client.StateManagement.Workflow;

namespace Uptime.Client.Application.Commands;

public record UpdateWorkflowTemplateCommand : IRequest<Result<bool>>
{
    public int TemplateId { get; init; }
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required string AssociationDataJson { get; init; }
}

public class UpdateWorkflowTemplateCommandHandler(IApiService apiService, IState<WorkflowState> workflowState)
    : IRequestHandler<UpdateWorkflowTemplateCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        User executor = User.OrSystemAccount(workflowState.Value.CurrentUser);

        var updateRequest = new WorkflowTemplateUpdateRequest {
            ExecutorSid = executor.Sid,
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            AssociationDataJson = request.AssociationDataJson
        }; 

        string url = ApiRoutes.WorkflowTemplates.UpdateTemplate.Replace("{templateId}", request.TemplateId.ToString());
        return await apiService.UpdateAsync(url, updateRequest, cancellationToken);
    }
}