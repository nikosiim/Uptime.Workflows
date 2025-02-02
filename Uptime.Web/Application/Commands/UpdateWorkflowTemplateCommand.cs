using MediatR;

namespace Uptime.Web.Application.Commands;

public record UpdateWorkflowTemplateCommand : IRequest<bool>
{
    public int TemplateId { get; init; }
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required string AssociationDataJson { get; init; }
}

public class UpdateWorkflowTemplateCommandHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<UpdateWorkflowTemplateCommand, bool>
{
    public async Task<bool> Handle(UpdateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new {
            request.TemplateName,
            request.WorkflowName,
            request.WorkflowBaseId,
            request.AssociationDataJson
        };

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            ApiRoutes.WorkflowTemplates.UpdateTemplate.Replace("{templateId}", request.TemplateId.ToString()),
            updateRequest,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}