using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Application.DTOs;

public record WorkflowTaskDetailsDto
{
    public required Guid TaskGuid { get; init; }
    public PrincipalId AssignedTo { get; init; }
    public PrincipalId AssignedBy { get; init; }
    public string Status { get; init; } = null!;
    public WorkflowTaskStatus InternalStatus { get; init; }
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public string? Document { get; init; }
    public string? PhaseId { get; init; }
    public int WorkflowId { get; init; }
    public required string WorkflowBaseId { get; init; }
}