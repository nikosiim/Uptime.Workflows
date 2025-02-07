using MediatR;

namespace Uptime.Web.Application.Commands;

public record StartWorkflowCommand : IRequest<bool>
{
    public required string Originator { get; set; }
    public required int DocumentId { get; set; }
    public required int WorkflowTemplateId { get; set; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class StartWorkflowCommandHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<StartWorkflowCommand, bool>
{
    public async Task<bool> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var payload = new { request.Originator, request.DocumentId, request.WorkflowTemplateId, Storage = request.Storage };

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(ApiRoutes.Workflows.StartWorkflow, payload, cancellationToken);
       
        return response.IsSuccessStatusCode;
    }
}