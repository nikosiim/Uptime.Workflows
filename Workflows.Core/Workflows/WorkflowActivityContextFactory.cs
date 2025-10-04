using System.Text.Json;
using Workflows.Core.Common;
using Workflows.Core.Enums;
using Workflows.Core.Extensions;

namespace Workflows.Core;

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