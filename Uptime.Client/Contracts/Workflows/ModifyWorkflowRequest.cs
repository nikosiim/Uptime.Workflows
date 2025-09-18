
namespace Uptime.Client.Contracts;

public record ModifyWorkflowRequest(string ExecutorSid, string? ModificationContext);