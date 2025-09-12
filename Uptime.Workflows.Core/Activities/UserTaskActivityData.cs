using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core;

public record UserTaskActivityData : IUserTaskActivityData
{
    public required PrincipalId AssignedToPrincipalId { get; init; }
    public required PrincipalId AssignedByPrincipalId { get; init; }
    public string? TaskDescription { get; init; }
    public DateTime? DueDate { get; init; }
}