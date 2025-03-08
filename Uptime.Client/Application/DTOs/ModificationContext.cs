namespace Uptime.Client.Application.DTOs;

public sealed class ModificationContext
{
    public int WorkflowId { get; set; }
    public string? Context { get; set; }

    //public required string PhaseId { get; set; }
    //public List<ContextTask>? ContextTasks { get; set; }
}

public record ContextTask
{
    public required string AssignedTo { get; set; }
    public required string TaskGuid { get; set; }
}