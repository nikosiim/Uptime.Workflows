using System.Net;
using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public sealed class OutboundNotificationService(WorkflowDbContext db) : IOutboundNotificationService
{
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public async Task LogNotificationAsync<TPayload>(
        OutboundEventType eventType,
        string endpointPath,
        TPayload payload,
        HttpStatusCode? statusCode,
        string? responseBody,
        Exception? error,
        string? uniqueKey = null,
        CancellationToken ct = default)
    {
        WorkflowId workflowId = payload switch
        {
            WorkflowStartedPayload started => started.WorkflowId,
            WorkflowCompletedPayload completed => completed.WorkflowId,
            TasksCreatedPayload created => created.WorkflowId,
            _ => throw new ArgumentException("Unknown payload type", nameof(payload))
        };

        string? phaseName = payload is TasksCreatedPayload t ? t.PhaseName : null;
        string payloadJson = JsonSerializer.Serialize(payload, _json);

        OutboundNotificationStatus status = error is null && statusCode.HasValue && (int)statusCode.Value >= 200 && (int)statusCode.Value < 300
            ? OutboundNotificationStatus.Sent
            : OutboundNotificationStatus.Failed;

        var notification = new OutboundNotification
        {
            EventType = eventType,
            WorkflowId = workflowId.Value,
            PhaseId = phaseName,
            EndpointPath = endpointPath,
            HttpStatusCode = statusCode.HasValue ? (int)statusCode.Value : 0,
            Status = status,
            PayloadJson = payloadJson,
            ResponseBody = Truncate(responseBody, 4000),
            LastError = error?.Message,
            AttemptCount = 1,
            CreatedAtUtc = DateTime.UtcNow,
            SentAtUtc = status == OutboundNotificationStatus.Sent ? DateTime.UtcNow : null,
            UniqueKey = uniqueKey
        };

        await db.OutboundNotifications.AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }

    private static string? Truncate(string? value, int max)
        => string.IsNullOrEmpty(value) ? value : value.Length <= max ? value : value[..max];
}
