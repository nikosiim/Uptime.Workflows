namespace Uptime.Client.Contracts;

public record WorkflowTaskResponse
{
    public Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public string Status { get; init; } = null!;
    public int InternalStatus { get; init; }
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public string? Document { get; init; }
    public int WorkflowId { get; init; }
    public string? PhaseId { get; init; }
    public string WorkflowBaseId { get; init; } = null!;
}