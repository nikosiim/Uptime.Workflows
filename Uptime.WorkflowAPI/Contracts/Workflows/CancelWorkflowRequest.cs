namespace Uptime.Workflows.Api.Contracts;

public record CancelWorkflowRequest(string ExecutorSid, string Comment);