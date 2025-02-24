namespace Uptime.Domain.Common;

public sealed class WorkflowDefinition
{
    public required string Id { get; init; }
    
    // A descriptive name, e.g. "Approval Workflow".
    public required string Name { get; init; }
    
    // The code-based workflow type or “base workflow” that your engine knows how to run:
    // e.g., "ApprovalWorkflow", "SigningWorkflow", etc.
    public required string WorkflowBaseId { get; init; }
    
    // Optionally: actions that the UI might display, e.g. Approve, Reject, etc.
    public IReadOnlyList<string> Actions { get; init; } = Array.Empty<string>();

    // The phase refers to the replicator that default type is sequential assignment
    public IReadOnlyList<TaskPhaseDefinition> TaskPhases { get; init; } = Array.Empty<TaskPhaseDefinition>();
}


public sealed record TaskPhaseDefinition
{
    public required string PhaseId { get; init; }  // "ApprovalPhase"
    public required string DisplayName { get; init; }  // "Approval"
    public bool SupportsSequential { get; init; }
    public bool SupportsParallel { get; init; }
    public required IReadOnlyList<string> Actions { get; init; }
}