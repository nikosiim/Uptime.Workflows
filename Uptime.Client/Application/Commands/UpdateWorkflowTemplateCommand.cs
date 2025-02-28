using MediatR;
using Uptime.Client.Application.Services;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Commands;

public record UpdateWorkflowTemplateCommand : IRequest<Result<bool>>
{
    public int TemplateId { get; init; }
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required string AssociationDataJson { get; init; }
}

public class UpdateWorkflowTemplateCommandHandler(IApiService apiService)
    : IRequestHandler<UpdateWorkflowTemplateCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new {
            request.TemplateName,
            request.WorkflowName,
            request.WorkflowBaseId,
            request.AssociationDataJson
        }; 

        string url = ApiRoutes.WorkflowTemplates.UpdateTemplate.Replace("{templateId}", request.TemplateId.ToString());
        return await apiService.UpdateAsync(url, updateRequest, cancellationToken);
    }
}