using Workflows.Application.Messaging;
using Workflows.Core.Common;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace Workflows.Application.Behaviors;

public sealed class PrincipalResolutionBehavior<TRequest, TResponse>(IPrincipalResolver resolver)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken ct)
    {
        if (request is IRequiresPrincipal principalRequest)
        {
            Principal? principal = await resolver.TryResolveBySidAsync(principalRequest.ExecutorSid, ct);
            if (principal == null)
            {
                throw new WorkflowValidationException(ErrorCode.NotFound, $"Principal with SID {principalRequest.ExecutorSid} not found.");
            }
        }

        return await next(ct);
    }
}