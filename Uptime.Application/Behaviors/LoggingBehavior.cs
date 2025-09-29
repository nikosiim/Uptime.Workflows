using Microsoft.Extensions.Logging;
using Uptime.Workflows.Application.Messaging;

namespace Uptime.Workflows.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken ct)
    {
        logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        TResponse response = await next(ct);
        logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return response;
    }
}