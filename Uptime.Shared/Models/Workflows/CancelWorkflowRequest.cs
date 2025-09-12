namespace Uptime.Shared.Models.Workflows;

public record CancelWorkflowRequest(string ExecutorSid, string Comment);