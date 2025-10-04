using Workflows.Core.Common;
using Workflows.Core.Enums;

namespace Workflows.Application.DTOs;

public record WorkflowTaskDetailsDto
{
    public required Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public string Status { get; init; } = null!;
    public WorkflowTaskStatus InternalStatus { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public DocumentId DocumentId { get; init; }
    public string? PhaseId { get; init; }
    public int WorkflowId { get; init; }
    public required string WorkflowBaseId { get; init; }
}