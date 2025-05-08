using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Application.Authentication;

public class TaskAccessRequirement : IAuthorizationRequirement;

public class TaskAccessHandler(IMembershipResolver resolver) : AuthorizationHandler<TaskAccessRequirement, WorkflowTask>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        TaskAccessRequirement requirement, WorkflowTask task)
    {
        foreach (AssigneeRef assignee in task.GetAssignments())
        {
            if (await resolver.IsMemberAsync(context.User, assignee))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}

public static class WorkflowTaskStorageExtensions
{
    private const string Key = "Assignments";

    public static IReadOnlyList<AssigneeRef> GetAssignments(this WorkflowTask task)
    {
        if (string.IsNullOrWhiteSpace(task.StorageJson))
            return [];

        using JsonDocument doc = JsonDocument.Parse(task.StorageJson);

        if (!doc.RootElement.TryGetProperty(Key, out JsonElement arr) || arr.ValueKind != JsonValueKind.Array)
            return [];

        var list = new List<AssigneeRef>(arr.GetArrayLength());
        foreach (JsonElement el in arr.EnumerateArray())
        {
            list.Add(new AssigneeRef
            {
                Type        = Enum.Parse<PrincipalKind>(el.GetProperty("Type").GetString()!, true),
                Id          = el.GetProperty("Id").GetString()!,
                DisplayName = el.TryGetProperty("DisplayName", out JsonElement dn) ? dn.GetString() : null
            });
        }
        return list;
    }
}