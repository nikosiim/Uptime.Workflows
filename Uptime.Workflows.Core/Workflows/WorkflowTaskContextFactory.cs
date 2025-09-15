using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core;

public static class WorkflowTaskContextFactory
{
    public static WorkflowTaskContext CreateNew(string? phaseId, 
        PrincipalId assignedTo, PrincipalId assignedBy, string? description, DateTime? dueDate)
    {
        var ctx = new WorkflowTaskContext
        {
            TaskGuid = Guid.NewGuid(),
            PhaseId = phaseId,
            DueDate = dueDate,
            AssignedToPrincipalId = assignedTo,
            AssignedByPrincipalId = assignedBy
        };

        ctx.SetTaskDescription(description);
        ctx.SetTaskStatus(WorkflowTaskStatus.NotStarted);
    
        return ctx;
    }

    public static WorkflowTaskContext FromDatabase(string? phaseId, Guid taskGuid, 
        PrincipalId assignedTo, PrincipalId assignedBy, DateTime? dueDate, string? storageJson)
    {
        return new WorkflowTaskContext
        {
            TaskGuid = taskGuid,
            PhaseId = phaseId,
            AssignedToPrincipalId = assignedTo,
            AssignedByPrincipalId = assignedBy,
            DueDate = dueDate,
            Storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(storageJson ?? "{}") ?? new Dictionary<string, string?>()
        };
    }
}