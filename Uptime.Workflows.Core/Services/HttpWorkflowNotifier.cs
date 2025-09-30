using System.Net;
using System.Net.Http.Json;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public class HttpWorkflowNotifier(HttpClient http, IOutboundNotificationService notifications) : IWorkflowOutboundNotifier
{
    public Task NotifyWorkflowStartedAsync(WorkflowStartedPayload payload, CancellationToken ct)
        => PostAndLogAsync("workflows/started", OutboundEventType.WorkflowStarted, payload, ct);

    public Task NotifyTasksCreatedAsync(TasksCreatedPayload payload, CancellationToken ct)
        => PostAndLogAsync("workflows/tasks-created", OutboundEventType.TasksCreated, payload, ct);

    public Task NotifyWorkflowCompletedAsync(WorkflowCompletedPayload payload, CancellationToken ct)
        => PostAndLogAsync("workflows/completed", OutboundEventType.WorkflowCompleted, payload, ct);

    private async Task PostAndLogAsync<TPayload>(string endpointPath, OutboundEventType eventType, TPayload payload, CancellationToken ct)
    {
        HttpStatusCode? statusCode = null;
        string? responseBody = null;
        Exception? error = null;

        try
        {
            using HttpResponseMessage resp = await http.PostAsJsonAsync(endpointPath, payload, cancellationToken: ct);
            statusCode = resp.StatusCode;
            responseBody = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                error = new HttpRequestException($"Outbound notify failed: {endpointPath} {(int)resp.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            error = ex;
        }

        await notifications.LogNotificationAsync(eventType, endpointPath, payload, statusCode, responseBody, error, null, ct);

        if (error is not null)
            throw error;
    }
}