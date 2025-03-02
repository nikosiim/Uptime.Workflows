
namespace Uptime.Shared.Models.Workflows;

public record ModifyWorkflowRequest
{
    public string? Executor { get; set; }
    public int WorkflowId { get; set; }
    public required string PhaseId { get; set; }
    public List<ContextTaskRequest>? ContextTasks { get; set; }
}

public record ContextTaskRequest
{
    public required string AssignedTo { get; set; }
    public required string TaskGuid { get; set; }
}