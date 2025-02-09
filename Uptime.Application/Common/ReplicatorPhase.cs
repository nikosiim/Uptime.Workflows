namespace Uptime.Application.Common;

/// <summary>
/// Tracks tasks for a specific workflow phase.
/// </summary>
public class ReplicatorPhase<TData>
{
    public required string PhaseName { get; init; }  // E.g., "ApprovalPhase"
    public required List<TData> TaskData { get; init; }
}