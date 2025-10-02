using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public record AlterTaskPayload : IUserActionPayload
{
    public required Guid TaskGuid { get; init; }
    public required PrincipalSid ExecutorSid { get; init; }
    public required string? PhaseId { get; init; }
    public required PrincipalSid AssignedToSid { get; init; }
    public required DateTimeOffset? DueDate { get; init; }
    public required string? Description { get; init; }
    public required string? StorageJson { get; init; }
    public Dictionary<string, string?> InputData { get; init; } = new();
}