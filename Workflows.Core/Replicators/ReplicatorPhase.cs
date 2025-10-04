using Workflows.Core.Enums;

namespace Workflows.Core;

/// <summary>
/// Tracks tasks for a specific workflow phase.
/// </summary>
public class ReplicatorPhase
{
    public required ReplicatorType Type { get; init; }
    public required string PhaseId { get; init; }
    public required List<WorkflowActivityContext> TaskContext { get; init; }
}