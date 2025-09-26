using Microsoft.Extensions.Logging;
using System.Reflection;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Application.Behaviors;

public sealed class ExceptionHandlingBehavior<TRequest, TResponse>(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> log)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken ct)
    {
        try
        {
            return await next(ct);
        }
        catch (WorkflowValidationException vex)
        {
            log.LogWarning("Validation error for {Request}: {Message} (Code: {Code})", typeof(TRequest).Name, vex.Message, vex.Error);
            return CreateFailure<TResponse>(vex.Error, vex.Message);
        }
        catch (OperationCanceledException)
        {
            log.LogInformation("Request cancelled: {Request}", typeof(TRequest).Name);
            return CreateCancelled<TResponse>();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unhandled exception while handling {Request}", typeof(TRequest).Name);
            return CreateFailure<TResponse>(ErrorCode.Unexpected, ex.Message);
        }
    }

    private static TRes CreateFailure<TRes>(ErrorCode code, string? details)
    {
        Type resType = typeof(TRes);
        MethodInfo? method = resType.GetMethod("Failure", BindingFlags.Public | BindingFlags.Static);
        if (method == null)
            throw new InvalidOperationException($"No static Failure method on {resType.Name}");

        return (TRes)method.Invoke(null, [code, details])!;
    }

    private static TRes CreateCancelled<TRes>()
    {
        Type resType = typeof(TRes);
        MethodInfo? method = resType.GetMethod("Cancelled", BindingFlags.Public | BindingFlags.Static);
        if (method == null)
            throw new InvalidOperationException($"No static Cancelled method on {resType.Name}");

        return (TRes)method.Invoke(null, null)!;
    }
}