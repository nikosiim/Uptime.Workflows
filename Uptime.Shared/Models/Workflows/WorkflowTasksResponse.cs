namespace Uptime.Shared.Models.Workflows;

public record WorkflowTasksResponse
{
    public Guid TaskId { get; init; }
    public int WorkflowId { get; init; }
    public string? AssignedTo { get; init; }
    public string Status { get; init; } = null!;
    public string? Comments { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}