using MediatR;
using Uptime.Client.Application.Services;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Commands;

public record StartWorkflowCommand : IRequest<Result<bool>>
{
    public required string Originator { get; set; }
    public required int DocumentId { get; set; }
    public required int WorkflowTemplateId { get; set; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class StartWorkflowCommandHandler(IApiService apiService)
    : IRequestHandler<StartWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var payload = new
        {
            request.Originator, 
            request.DocumentId, 
            request.WorkflowTemplateId, 
            request.Storage
        };

        return await apiService.PostAsJsonAsync(ApiRoutes.Workflows.StartWorkflow, payload, cancellationToken);
    }
}