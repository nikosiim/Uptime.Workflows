using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Application.Behaviors;

public sealed class PrincipalResolutionBehavior<TRequest, TResponse>(IPrincipalResolver resolver)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        if (request is IRequiresPrincipal principalRequest)
        {
            Principal? principal = await resolver.ResolveBySidAsync(principalRequest.ExecutorSid, cancellationToken);
            if (principal == null)
            {
                throw new WorkflowValidationException(ErrorCode.NotFound, $"Principal with SID {principalRequest.ExecutorSid} not found.");
            }

            principalRequest.ExecutedBy = principal;
        }

        return await next(cancellationToken);
    }
}