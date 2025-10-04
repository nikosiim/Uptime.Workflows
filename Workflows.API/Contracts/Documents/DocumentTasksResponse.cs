namespace Workflows.Api.Contracts;

public record DocumentTasksResponse
{
    public int TaskId { get; init; }
    public int WorkflowId { get; init; }
    public string? AssignedTo { get; init; }
    public required string WorkflowTaskStatus { get; init; }
    public string? TaskDescription { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
}