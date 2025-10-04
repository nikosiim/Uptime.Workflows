using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace MembershipValidationService;

public class WorkflowNotifierFunctions(ILogger<WorkflowNotifierFunctions> logger)
{
    // Helper to simulate random delay
    private static async Task SimulateSharePointDelayAsync()
    {
        int delayMs = Random.Shared.Next(0, 3000);
        await Task.Delay(delayMs);
    }

    [Function("WorkflowsStarted")]
    public async Task<HttpResponseData> WorkflowsStarted(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workflows/started")]
        HttpRequestData req)
    {
        logger.LogInformation("WorkflowsStarted called.");
        await SimulateSharePointDelayAsync();
        return CreateOkResponse(req);
    }

    [Function("WorkflowsTasksCreated")]
    public async Task<HttpResponseData> WorkflowsTasksCreated(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workflows/tasks-created")]
        HttpRequestData req)
    {
        logger.LogInformation("WorkflowsTasksCreated called.");
        await SimulateSharePointDelayAsync();
        return CreateOkResponse(req);
    }

    [Function("WorkflowsCompleted")]
    public async Task<HttpResponseData> WorkflowsCompleted(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workflows/completed")]
        HttpRequestData req)
    {
        logger.LogInformation("WorkflowsCompleted called.");
        await SimulateSharePointDelayAsync();
        return CreateOkResponse(req);
    }

    [Function("WorkflowsTaskUpdated")]
    public async Task<HttpResponseData> WorkflowsTaskUpdated(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workflows/task-updated")]
        HttpRequestData req)
    {
        logger.LogInformation("WorkflowsTaskUpdated called.");
        await SimulateSharePointDelayAsync();
        return CreateOkResponse(req);
    }

    private static HttpResponseData CreateOkResponse(HttpRequestData req)
    {
        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteString("OK");
        return response;
    }
}
