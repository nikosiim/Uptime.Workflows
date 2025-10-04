using System.Net;

namespace Uptime.Workflows.Core.Interfaces;

public interface IOutboundNotificationService
{
    Task LogNotificationAsync<TPayload>(
        string eventName,
        string endpointPath,
        TPayload payload,
        HttpStatusCode? statusCode,
        string? responseBody,
        Exception? error,
        CancellationToken ct = default) where TPayload : IOutboundNotificationPayload;
}