using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public sealed class SharePointNotifierOptions
{
    public string RelativePath { get; set; } = "/_vti_bin/Uptime.Notifier/WorkflowNotify.svc/";
    public string Started { get; set; } = "workflows/started";
    public string TasksCreated { get; set; } = "workflows/tasks-created";
    public string Completed { get; set; } = "workflows/completed";
}

public sealed class HttpWorkflowNotifier(HttpClient http, IOutboundNotificationService notifications, IOptions<SharePointNotifierOptions> opt)
    : IWorkflowOutboundNotifier
{
    private readonly SharePointNotifierOptions _opt = opt.Value;

    public Task NotifyWorkflowStartedAsync(WorkflowStartedPayload payload, CancellationToken ct)
        => PostAndLogAsync(BuildAbsolute(payload, _opt.Started), OutboundEventType.WorkflowStarted, payload, ct);

    public Task NotifyTasksCreatedAsync(TasksCreatedPayload payload, CancellationToken ct)
        => PostAndLogAsync(BuildAbsolute(payload, _opt.TasksCreated), OutboundEventType.TasksCreated, payload, ct);

    public Task NotifyWorkflowCompletedAsync(WorkflowCompletedPayload payload, CancellationToken ct)
        => PostAndLogAsync(BuildAbsolute(payload, _opt.Completed), OutboundEventType.WorkflowCompleted, payload, ct);

    private Uri BuildAbsolute(IOutboundNotificationPayload payload, string endpointSegment)
    {
        // Ensure / at end of site, and at end of relative path
        string site = payload.SourceSiteUrl.TrimEnd('/') + "/";
        string rel = _opt.RelativePath.TrimStart('/').TrimEnd('/') + "/";

        return new Uri(new Uri(site), rel + endpointSegment);
    }

    private async Task PostAndLogAsync<TPayload>(Uri absoluteEndpoint, OutboundEventType eventType, TPayload payload, CancellationToken ct)
        where TPayload : IOutboundNotificationPayload
    {
        HttpStatusCode? statusCode = null;
        string? responseBody = null;
        Exception? error = null;

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, absoluteEndpoint);
            req.Content = JsonContent.Create(payload);

            // Helpful for diagnostics/routing on receiver
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

        await notifications.LogNotificationAsync(eventType, absoluteEndpoint.ToString(), payload, statusCode, responseBody, error, ct);

        if (error is not null)
            throw error;
    }
}