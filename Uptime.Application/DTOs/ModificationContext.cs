namespace Uptime.Application.DTOs;

public record ModificationContext
{
    public required string WorkflowId { get; set; }
    public required string WorkflowBaseId { get; set; }
    public required string PhaseId { get; set; }
    public List<TaskItem>? TaskItems { get; set; }
}

public record TaskItem
{
    public required string AssignedTo { get; set; }
    public required string TaskGuid { get; set; }
}