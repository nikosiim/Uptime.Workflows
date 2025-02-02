using MediatR;

namespace Uptime.Web.Application.Commands;

public record DeleteWorkflowTemplateCommand(int TemplateId) : IRequest<bool>;

public class DeleteWorkflowTemplateCommandHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<DeleteWorkflowTemplateCommand, bool>
{
    public async Task<bool> Handle(DeleteWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);

        HttpResponseMessage response = await httpClient.DeleteAsync(
            ApiRoutes.WorkflowTemplates.DeleteTemplate.Replace("{templateId}", request.TemplateId.ToString()),
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}