namespace Uptime.Client.Contracts;

public record CancelWorkflowRequest(string ExecutorSid, string Comment);