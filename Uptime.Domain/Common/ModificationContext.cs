namespace Uptime.Domain.Common;

public record ModificationContext
{
    public string? Executor { get; set; }
    public int WorkflowId { get; set; }
    public required string PhaseId { get; set; }
    public List<ContextTask>? ContextTasks { get; set; }
}

public record ContextTask
{
    public required string AssignedTo { get; set; }
    public required string TaskGuid { get; set; }
}