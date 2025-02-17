using MediatR;

namespace Uptime.Web.Application.Commands;

public record AlterTaskCommand : IRequest<bool>
{
    public int TaskId { get; init; }
    public int WorkflowId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class AlterTaskCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<AlterTaskCommand, bool>
{
    public async Task<bool> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        var payload = new { request.WorkflowId, request.Storage };

        HttpClient httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            ApiRoutes.Tasks.AlterTask.Replace("{taskId}", request.TaskId.ToString()), 
            payload, 
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
        }

        return false;
    }
}