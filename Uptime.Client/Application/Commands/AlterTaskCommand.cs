using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;

namespace Uptime.Client.Application.Commands;

public record AlterTaskCommand : IRequest<Result<bool>>
{
    public User? Executor { get; init; }
    public required Guid TaskGuid { get; init; }
    public required WorkflowEventType Action { get; init; }
    public Dictionary<string, string?> Input { get; init; } = new();
}

public class AlterTaskCommandHandler(IApiService apiService) : IRequestHandler<AlterTaskCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        User executor = User.OrSystemAccount(request.Executor);

        var payload = new AlterTaskRequest(executor.Sid, request.Action)
        {
            Input = request.Input
        };
        
        string url = ApiRoutes.Tasks.AlterTask.Replace("{taskGuid}", request.TaskGuid.ToString());
        return await apiService.PostAsJsonAsync(url, payload, cancellationToken);
    }
}