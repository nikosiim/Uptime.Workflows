using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core;

public class WorkflowTaskContext : IWorkflowTaskContext
{
    public string? PhaseId { get; init; }
    public required Guid TaskGuid { get; init; }
    public PrincipalId AssignedToPrincipalId { get; init; }
    public PrincipalId AssignedByPrincipalId { get; init; }
    public DateTime? DueDate { get; init; }

    // Only storage for business/task fields (private/protected/internal set)

    [Obsolete("Use strongly-typed extension methods instead.")]
    public Dictionary<string, string?> Storage { get; init; } = new();
}