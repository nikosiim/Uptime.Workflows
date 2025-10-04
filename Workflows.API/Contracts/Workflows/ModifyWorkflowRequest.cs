
namespace Workflows.Api.Contracts;

public record ModifyWorkflowRequest(string ExecutorSid, string? ModificationContext);