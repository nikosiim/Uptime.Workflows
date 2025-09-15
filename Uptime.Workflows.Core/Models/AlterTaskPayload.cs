using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public record AlterTaskPayload : IUserActionPayload
{
    public required Principal ExecutedBy { get; init; }
    public required string? PhaseId { get; init; }
    public required Guid TaskGuid { get; init; }
    public required PrincipalId AssignedTo { get; init; }
    public required DateTime? DueDate { get; init; }
    public required string? Description { get; init; }
    public required string? StorageJson { get; init; }
    public Dictionary<string, string?> InputData { get; init; } = new();
}