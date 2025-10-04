using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public sealed class LoggingWorkflowNotifier(ILogger<LoggingWorkflowNotifier> logger) : IWorkflowOutboundNotifier
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public Task NotifyWorkflowStartedAsync(WorkflowStartedPayload payload, CancellationToken ct)
    {
        logger.LogWarning("WF-STARTED: WorkflowId={WorkflowId}, Type={WorkflowType}, StartedBySid={StartedBySid}, Assignees={AssigneesJson}, At={StartedAtUtc}",
            payload.WorkflowId,
            payload.WorkflowType,
            payload.StartedBySid.Value,
            JsonSerializer.Serialize(payload.Assignees, JsonOptions),
            payload.StartedAtUtc);

        return Task.CompletedTask;
    }

    public Task NotifyTasksCreatedAsync(TasksCreatedPayload payload, CancellationToken ct)
    {
        logger.LogWarning("WF-TASKS-CREATED: WorkflowId={WorkflowId}, Type={WorkflowType}, Phase={PhaseName}, Parallel={IsParallel}, Count={Count}, Tasks={TasksJson}",
            payload.WorkflowId,
            payload.WorkflowType,
            payload.PhaseName,
            payload.IsParallelPhase,
            payload.Tasks.Count,
            JsonSerializer.Serialize(payload.Tasks, JsonOptions));

        return Task.CompletedTask;
    }

    public Task NotifyWorkflowCompletedAsync(WorkflowCompletedPayload payload, CancellationToken ct)
    {
        logger.LogWarning("WF-COMPLETED: WorkflowId={WorkflowId}, Type={WorkflowType}, Outcome={Outcome}, Assignees={AssigneesJson}",
            payload.WorkflowId,
            payload.WorkflowType,
            payload.Outcome,
            JsonSerializer.Serialize(payload.Assignees, JsonOptions));

        return Task.CompletedTask;
    }
}
