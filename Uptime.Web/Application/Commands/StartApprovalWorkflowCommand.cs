using MediatR;

namespace Uptime.Web.Application.Commands;

public record ApprovalWorkflowPayload
{
    public required int WorkflowTemplateId { get; set; }
    public required int DocumentId { get; set; }
    public required string Originator { get; set; }
    public required List<string> Executors { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}

public record StartApprovalWorkflowCommand(ApprovalWorkflowPayload Payload) : IRequest<bool>;

public class StartApprovalWorkflowCommandHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<StartApprovalWorkflowCommand, bool>
{
    public async Task<bool> Handle(StartApprovalWorkflowCommand request, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(ApiRoutes.Workflows.StartApprovalWorkflow, request.Payload, cancellationToken);
       
        return response.IsSuccessStatusCode;
    }
}