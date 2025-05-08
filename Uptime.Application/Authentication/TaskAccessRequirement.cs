using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Application.Authentication;

public sealed class TaskAccessRequirement : IAuthorizationRequirement;

public sealed class TaskAccessHandler(IMembershipResolver resolver, ILogger<TaskAccessHandler> log)
    : AuthorizationHandler<TaskAccessRequirement, WorkflowTask>
{
    // the “admin” group that may always finish a task
    private const string AdminGroupName = "WF_Admins";   // ← move to config if you wish

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TaskAccessRequirement requirement, WorkflowTask task)
    {
        CancellationToken ct = context.Resource as CancellationToken? ?? CancellationToken.None;

        // 1) Who is calling?
        string? caller = context.User.ToUpn();
        if (caller is null)
        {
            log.LogWarning("Unable to identify caller for task {TaskId}", task.Id);
            return; // → requirement fails
        }

        // 2) The assigner may always complete
        if (caller.Equals(task.AssignedBy, StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        // 3) Admin group override
        if (await resolver.IsUserInGroupAsync(caller, AdminGroupName, ct))
        {
            context.Succeed(requirement);
            return;
        }

        // 4) Iterate over Assignments list from StorageJson
        foreach (AssigneeRef a in task.GetAssignments())
        {
            bool allowed = a.Kind switch
            {
                PrincipalKind.User => await resolver.IsSameUserOrSubstituteAsync(caller, a.IdOrName, ct),
                PrincipalKind.Group => await resolver.IsUserInGroupAsync(caller, a.IdOrName, ct),
                _ => false
            };

            if (allowed)
            {
                context.Succeed(requirement);
                return;
            }
        }

        // 5) If we reach here → not authorized (context.Succeed not called)
    }
}