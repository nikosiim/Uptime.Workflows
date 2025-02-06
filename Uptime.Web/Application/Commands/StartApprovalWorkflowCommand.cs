using MediatR;

namespace Uptime.Web.Application.Commands;

public record ApprovalWorkflowPayload
{
    public required string Originator { get; set; }
    public required int DocumentId { get; set; }
    public required int WorkflowTemplateId { get; set; }
    public Dictionary<string, object?> Data { get; init; } = new();
}

public record StartApprovalWorkflowCommand(ApprovalWorkflowPayload Payload) : IRequest<bool>;

public class StartApprovalWorkflowCommandHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<StartApprovalWorkflowCommand, bool>
{
    public async Task<bool> Handle(StartApprovalWorkflowCommand request, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(ApiRoutes.Workflows.StartWorkflow, request.Payload, cancellationToken);
       
        return response.IsSuccessStatusCode;
    }
}