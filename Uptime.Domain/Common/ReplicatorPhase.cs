using Uptime.Domain.Enums;

namespace Uptime.Domain.Common;

/// <summary>
/// Tracks tasks for a specific workflow phase.
/// </summary>
public class ReplicatorPhase
{
    public required ReplicatorType Type { get; init; }
    public required string PhaseName { get; init; }
    public required List<object> TaskData { get; init; }
}