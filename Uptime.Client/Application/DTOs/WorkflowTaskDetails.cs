using Uptime.Client.Contracts;

namespace Uptime.Client.Application.DTOs;

public record WorkflowTaskDetails
{
    public required Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public string? Document { get; init; }
    public int DocumentId { get; init; }
    public int WorkflowId { get; init; }
    public string? Status { get; init; }
    public string? PhaseId { get; init; }
    public required string WorkflowBaseId { get; init; }
    public WorkflowTaskStatus InternalStatus { get; init; }
}