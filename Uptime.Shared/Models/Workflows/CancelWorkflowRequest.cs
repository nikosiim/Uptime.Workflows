namespace Uptime.Shared.Models.Workflows;

public record CancelWorkflowRequest
{
    public required string Executor { get; init; }
    public required int WorkflowId { get; init; }
    public required string Comment { get; init; }
}