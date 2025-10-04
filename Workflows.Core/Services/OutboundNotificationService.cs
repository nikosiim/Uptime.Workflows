using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using Workflows.Core.Data;
using Workflows.Core.Enums;
using Workflows.Core.Interfaces;

namespace Workflows.Core.Services;

public sealed class OutboundNotificationService(WorkflowDbContext db) : IOutboundNotificationService
{
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    
    public async Task LogNotificationAsync<TPayload>(string eventName, string endpointPath,
        TPayload payload, HttpStatusCode? statusCode, string? responseBody, Exception? error, CancellationToken ct = default) 
        where TPayload : IOutboundNotificationPayload
    {
        string payloadJson = JsonSerializer.Serialize((object)payload, _json);

        OutboundNotificationStatus status = 
            error is null && statusCode.HasValue && (int)statusCode.Value >= 200 && (int)statusCode.Value < 300
                ? OutboundNotificationStatus.Sent
                : OutboundNotificationStatus.Failed;

        OutboundNotification? existing = await db.OutboundNotifications.SingleOrDefaultAsync(n => n.UniqueKey == payload.UniqueKey, ct);
        if (existing is null)
        {
            var notification = new OutboundNotification
            {
                EventName = eventName,
                WorkflowId = payload.WorkflowId.Value,
                HttpStatusCode = statusCode.HasValue ? (int)statusCode.Value : 0,
                Status = status,
                OccurredAtUtc = payload.OccurredAtUtc,
                PayloadJson = payloadJson,
                ResponseBody = Truncate(responseBody, 4000),
                LastError = error?.Message,
                AttemptCount = 1,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                SentAtUtc = status == OutboundNotificationStatus.Sent ? DateTimeOffset.UtcNow : null,
                UniqueKey = payload.UniqueKey
            };

            await db.OutboundNotifications.AddAsync(notification, ct);
        }
        else
        {
            existing.AttemptCount += 1;
            existing.Status = status;
            existing.HttpStatusCode = statusCode.HasValue ? (int)statusCode.Value : 0;
            existing.ResponseBody = Truncate(responseBody, 4000);
            existing.LastError = error?.Message;
            existing.PayloadJson = payloadJson;
            existing.SentAtUtc = status == OutboundNotificationStatus.Sent ? DateTimeOffset.UtcNow : null;
        }

        await db.SaveChangesAsync(ct);
    }

    private static string? Truncate(string? value, int max)
        => string.IsNullOrWhiteSpace(value) ? value : value.Length <= max ? value : value[..max];
}