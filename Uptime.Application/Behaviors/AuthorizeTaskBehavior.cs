using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.Authentication;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Behaviors;

public sealed class AuthorizeTaskBehavior<TReq, TRes>(WorkflowDbContext db, IAuthorizationService auth) 
    : IPipelineBehavior<TReq, TRes> where TReq : IRequest<TRes>, ITaskAuthorizationRequest
{
    public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        WorkflowTask? task = await db.WorkflowTasks
            .Include(t => t.Workflow).ThenInclude(w => w.WorkflowTemplate)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId.Value, ct);

        if (task is null)
            return (TRes)(object) Result<Unit>.Failure(ErrorCode.NotFound);

        AuthorizationResult ar = await auth.AuthorizeAsync(request.Caller, task, new TaskAccessRequirement());
        if (!ar.Succeeded)
            return (TRes)(object) Result<Unit>.Failure(ErrorCode.Forbidden);

        return await next(ct);
    }
}