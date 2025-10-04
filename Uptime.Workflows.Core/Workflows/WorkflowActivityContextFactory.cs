using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;

namespace Uptime.Workflows.Core;

public static class WorkflowActivityContextFactory
{
    public static WorkflowActivityContext CreateNew(string? phaseId, PrincipalSid assignedToSid, string? description, DateTimeOffset? dueDate)
    {
        var ctx = new WorkflowActivityContext
        {
            TaskGuid = Guid.CreateVersion7(),
            PhaseId = phaseId,
            DueDate = dueDate,
            Description = description,
            AssignedToSid = assignedToSid
        };

        ctx.SetTaskStatus(WorkflowTaskStatus.NotStarted);
    
        return ctx;
    }

    public static WorkflowActivityContext FromDatabase(Guid taskGuid, string? phaseId, string? description,
        PrincipalSid assignedToSid, DateTimeOffset? dueDate, string? storageJson)
    {
        return new WorkflowActivityContext
        {
            TaskGuid = taskGuid,
            PhaseId = phaseId,
            AssignedToSid = assignedToSid,
            DueDate = dueDate,
            Description = description,
            Storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(storageJson ?? "{}") ?? new Dictionary<string, string?>()
        };
    }
}