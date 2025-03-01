namespace Uptime.Shared.Models.Workflows;

public record CancelWorkflowRequest(string Executor, string Comment);