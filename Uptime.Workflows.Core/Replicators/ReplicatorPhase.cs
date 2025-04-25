using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core;

/// <summary>
/// Tracks tasks for a specific workflow phase.
/// </summary>
public class ReplicatorPhase
{
    public required ReplicatorType Type { get; init; }
    public required string PhaseName { get; init; }
    public required List<object> TaskData { get; init; }
}