namespace Uptime.Shared.Models.Documents;

public record DocumentTasksResponse
{
    public int TaskId { get; init; }
    public int WorkflowId { get; init; }
    public string? AssignedTo { get; init; }
    public required string WorkflowTaskStatus { get; init; }
    public string? TaskDescription { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? EndDate { get; init; }
}