using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Services;

public sealed class HttpWorkflowNotifier(
    HttpClient http, 
    IOutboundNotificationService notifications, 
    IOptions<HttpWorkflowNotifierOptions> opt) : IWorkflowOutboundNotifier
{
    private readonly HttpWorkflowNotifierOptions _opt = opt.Value;

    public Task NotifyAsync(string eventName, IOutboundNotificationPayload payload, CancellationToken ct = default)
        => PostAndLogAsync(BuildAbsolute(payload, eventName), eventName, payload, ct);

    private Uri BuildAbsolute(IOutboundNotificationPayload payload, string eventName)
    {
        if (string.IsNullOrWhiteSpace(payload.SourceSiteUrl))
            throw new InvalidOperationException($"[HttpWorkflowNotifier] SourceSiteUrl is required for event '{eventName}' but was null or empty.");
        
        // Ensure / at end of site, and at end of relative path
        string site = payload.SourceSiteUrl.TrimEnd('/') + "/";
        string rel = _opt.RelativePath.TrimStart('/').TrimEnd('/') + "/";

        return new Uri(new Uri(site), rel + eventName);
    }

    private async Task PostAndLogAsync<TPayload>(Uri absoluteEndpoint, string eventName, TPayload payload, CancellationToken ct) 
        where TPayload : IOutboundNotificationPayload
    {
        HttpStatusCode? statusCode = null;
        string? responseBody = null;
        Exception? error = null;

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, absoluteEndpoint);
            req.Content = JsonContent.Create(payload);

            req.Headers.TryAddWithoutValidation("X-SharePoint-SiteUrl", payload.SourceSiteUrl);
            req.Headers.Accept.ParseAdd("application/json");

            using HttpResponseMessage resp = await http.SendAsync(req, ct);
            statusCode = resp.StatusCode;
            responseBody = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                error = new HttpRequestException($"Outbound notify failed: {absoluteEndpoint} {(int)resp.StatusCode}");
        }
        catch (Exception ex)
        {
            error = ex;
        }

        await notifications.LogNotificationAsync(
            eventName,
            absoluteEndpoint.ToString(),
            payload,
            statusCode,
            responseBody,
            error,
            ct);

        if (error is not null)
            throw error;
    }
}