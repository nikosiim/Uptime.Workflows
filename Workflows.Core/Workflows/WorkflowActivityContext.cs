using Workflows.Core.Common;
using Workflows.Core.Interfaces;

namespace Workflows.Core;

public class WorkflowActivityContext : IWorkflowActivityContext
{
    public string? PhaseId { get; init; }
    public required Guid TaskGuid { get; init; }
    public required PrincipalSid AssignedToSid { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public string? Description { get; init; }

    // Only storage for business/task fields (private/protected/internal set)

    /// <summary>
    /// Internal key-value storage for activity-specific state.
    /// <para>
    /// ⚠️ Do not access this dictionary directly.  
    /// Always use strongly-typed extension methods (e.g., <c>SetTaskOutcome()</c>, <c>GetTaskStatus()</c>)  
    /// from <c>ActivityContextExtensionsCore</c> to read/write data.
    /// </para>
    /// <para>
    /// This storage is persisted between workflow invocations.
    /// Keys must be stable across versions.
    /// </para>
    /// </summary>
    public Dictionary<string, string?> Storage { get; init; } = new();
}