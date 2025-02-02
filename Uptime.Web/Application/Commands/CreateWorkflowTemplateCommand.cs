using MediatR;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.Web.Application.Commands;

public record CreateWorkflowTemplateCommand : IRequest<int>
{
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required int LibraryId { get; init; }
    public string? AssociationDataJson { get; init; }
}

public class CreateWorkflowTemplateCommandHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<CreateWorkflowTemplateCommand, int>
{
    public async Task<int> Handle(CreateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new {
            request.TemplateName,
            request.WorkflowName,
            request.WorkflowBaseId,
            request.LibraryId,
            request.AssociationDataJson
        };

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(ApiRoutes.WorkflowTemplates.CreateTemplate, createRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to create workflow template.");
        }

        var result = await response.Content.ReadFromJsonAsync<CreateWorkflowTemplateResponse>(cancellationToken: cancellationToken);

        if (result is not { Id: > 0 })
        {
            throw new Exception("Invalid response received when creating workflow template.");
        }

        return result.Id;
    }
}