using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;

namespace Uptime.Workflows.Core;

public static class WorkflowActivityContextFactory
{
    public static WorkflowActivityContext CreateNew(string? phaseId, 
        PrincipalId assignedTo, PrincipalId assignedBy, string? description, DateTime? dueDate)
    {
        var ctx = new WorkflowActivityContext
        {
            TaskGuid = Guid.CreateVersion7(),
            PhaseId = phaseId,
            DueDate = dueDate,
            Description = description,
            AssignedToPrincipalId = assignedTo,
            AssignedByPrincipalId = assignedBy
        };

        ctx.SetTaskStatus(WorkflowTaskStatus.NotStarted);
    
        return ctx;
    }

    public static WorkflowActivityContext FromDatabase(Guid taskGuid, string? phaseId, string? description,
        PrincipalId assignedTo, PrincipalId assignedBy, DateTime? dueDate, string? storageJson)
    {
        return new WorkflowActivityContext
        {
            TaskGuid = taskGuid,
            PhaseId = phaseId,
            AssignedToPrincipalId = assignedTo,
            AssignedByPrincipalId = assignedBy,
            DueDate = dueDate,
            Description = description,
            #pragma warning disable CS0618
            Storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(storageJson ?? "{}") ?? new Dictionary<string, string?>()
            #pragma warning restore CS0618
        };
    }
}