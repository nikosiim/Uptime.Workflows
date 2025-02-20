using MediatR;

namespace Uptime.Web.Application.Commands;

public record AlterTaskCommand : IRequest<bool>
{
    public int TaskId { get; init; }
    public int WorkflowId { get; init; }
    public Dictionary<string, string?> Input { get; init; } = new();
}

public class AlterTaskCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<AlterTaskCommand, bool>
{
    public async Task<bool> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        var payload = new { request.WorkflowId, Input = request.Input };

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            ApiRoutes.Tasks.AlterTask.Replace("{taskId}", request.TaskId.ToString()), 
            payload, 
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}