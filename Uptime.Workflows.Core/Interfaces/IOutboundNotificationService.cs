using System.Net;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Interfaces;

public interface IOutboundNotificationService
{
    Task LogNotificationAsync<TPayload>(
        OutboundEventType eventType,
        string endpointPath,
        TPayload payload,
        HttpStatusCode? statusCode,
        string? responseBody,
        Exception? error,
        string? uniqueKey = null,
        CancellationToken ct = default);
}