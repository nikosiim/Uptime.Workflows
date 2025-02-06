using Uptime.Shared.Enums;

namespace Uptime.Shared.Models.Documents;

public record DocumentTasksResponse
{
    public int TaskId { get; init; }
    public int WorkflowId { get; init; }
    public string? AssignedTo { get; init; }
    public WorkflowTaskStatus Status { get; init; }
    public string? TaskDescription { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? EndDate { get; init; }
}