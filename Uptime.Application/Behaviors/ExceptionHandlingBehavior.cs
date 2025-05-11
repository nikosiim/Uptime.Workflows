using MediatR;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Application.Behaviors;

public sealed class ExceptionHandlingBehavior<TReq, TRes>(ILogger<ExceptionHandlingBehavior<TReq, TRes>> log)
    : IPipelineBehavior<TReq, Result<TRes>> where TReq : IRequest<Result<TRes>>
{
    public async Task<Result<TRes>> Handle(TReq request, RequestHandlerDelegate<Result<TRes>> next, CancellationToken ct)
    {
        try
        {
            return await next(ct);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unhandled exception while handling {Request}", typeof(TReq).Name);
            return Result<TRes>.Failure(ErrorCode.Unexpected);
        }
    }
}