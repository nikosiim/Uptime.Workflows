using MediatR;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Application.Behaviors;

public sealed class PrincipalResolutionBehavior<TReq, TRes>(IPrincipalResolver resolver, ILogger<PrincipalResolutionBehavior<TReq, TRes>> log)
    : IPipelineBehavior<TReq, Result<TRes>> where TReq : IRequest<Result<TRes>>
{
    public async Task<Result<TRes>> Handle(TReq request, RequestHandlerDelegate<Result<TRes>> next, CancellationToken ct)
    {
        if (request is not IPrincipalRequest principalRequest)
            return await next(ct);

        Principal? principal = await resolver.ResolveBySidAsync(principalRequest.ExecutedBySid, ct);
        if (principal is null)
        {
            log.LogWarning("Principal with SID {ExecutedBySid} not found.", principalRequest.ExecutedBySid);
            return Result<TRes>.Failure(ErrorCode.NotFound, $"Principal with SID {principalRequest.ExecutedBySid} not found.");
        }

        principalRequest.ExecutedBy = principal;
        return await next(ct);
    }
}