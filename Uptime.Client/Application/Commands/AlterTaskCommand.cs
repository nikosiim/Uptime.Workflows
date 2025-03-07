using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;

namespace Uptime.Client.Application.Commands;

public record AlterTaskCommand : IRequest<Result<bool>>
{
    public int TaskId { get; init; }
    public Dictionary<string, string?> Input { get; init; } = new();
}

public class AlterTaskCommandHandler(IApiService apiService) : IRequestHandler<AlterTaskCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Tasks.AlterTask.Replace("{taskId}", request.TaskId.ToString());
        var payload = new { request.Input };

        return await apiService.PostAsJsonAsync(url, payload, cancellationToken);
    }
}