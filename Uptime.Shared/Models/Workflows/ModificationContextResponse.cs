
namespace Uptime.Shared.Models.Workflows;

public record ModificationContextResponse
{
    public required string WorkflowId { get; set; }
    public required string WorkflowBaseId { get; set; }
    public required string PhaseId { get; set; }
    public List<TaskResponse>? TaskItems { get; set; }
}

public record TaskResponse
{
    public required string AssignedTo { get; set; }
    public required string TaskGuid { get; set; }
}