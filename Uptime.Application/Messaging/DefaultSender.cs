using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace Uptime.Workflows.Application.Messaging;

internal sealed class DefaultSender(IServiceProvider sp) : ISender
{
    private readonly ILogger<DefaultSender>? _logger = sp.GetService<ILogger<DefaultSender>>();
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object, CancellationToken, Task<object>>> HandlerCache = new();
    
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        Type requestType = request.GetType();
        Type responseType = typeof(TResponse);
        (Type requestType, Type responseType) key = (requestType, responseType);

        // Resolve handler
        Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        object handler = sp.GetService(handlerType) ?? throw new InvalidOperationException($"No handler for {requestType.Name}.");

        // Get or create cached handler delegate
        Func<object, object, CancellationToken, Task<object>> handlerFunc = HandlerCache.GetOrAdd(key, _ => BuildHandlerDelegate(handlerType));

        // Compose pipeline behaviors (reflection for behaviors, as they're rare)
        Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        IEnumerable<object?> behaviors = sp.GetServices(behaviorType);
        
        // Compose pipeline (reverse for correct nesting)
        Func<CancellationToken, Task<TResponse>> pipeline = async c => (TResponse)await handlerFunc(handler, request, c);

        foreach (object behavior in behaviors.Cast<object>().Reverse())
        {
            object b = behavior; // closure capture
            Func<CancellationToken, Task<TResponse>> prev = pipeline;
            pipeline = ct2 =>
            {
                // All IPipelineBehavior<TReq, TRes> have same signature
                MethodInfo method = b.GetType().GetMethod("Handle")!;
                return (Task<TResponse>)method.Invoke(b, [request, prev, ct2])!;
            };
        }

        return await pipeline(ct);
    }

    private static Func<object, object, CancellationToken, Task<object>> BuildHandlerDelegate(Type handlerType)
    {
        MethodInfo handleMethod = handlerType.GetMethod("Handle")!;

        return async (handler, request, ct) =>
        {
            var task = (Task)handleMethod.Invoke(handler, [request, ct])!;
            await task.ConfigureAwait(false);
            PropertyInfo? resultProperty = task.GetType().GetProperty("Result");
            return resultProperty!.GetValue(task)!;
        };
    }
}