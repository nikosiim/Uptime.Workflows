using Microsoft.AspNetCore.Authorization;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace Uptime.Application.Authentication;

public class TaskAccessRequirement : IAuthorizationRequirement;

public class TaskAccessHandler(IMembershipResolver resolver) : AuthorizationHandler<TaskAccessRequirement, WorkflowTask>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        TaskAccessRequirement requirement, WorkflowTask task)
    {
        foreach (AssigneeRef assignee in task.Storage.GetAssignments())
        {
            if (await resolver.IsMemberAsync(context.User, assignee))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}