using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public class WorkflowActivityContext : IWorkflowActivityContext
{
    public string? PhaseId { get; init; }
    public required Guid TaskGuid { get; init; }
    public required PrincipalSid AssignedToSid { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public string? Description { get; init; }

    // Only storage for business/task fields (private/protected/internal set)

    [Obsolete("Use strongly-typed extension methods instead.")]
    public Dictionary<string, string?> Storage { get; init; } = new();
}