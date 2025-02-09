using Uptime.Domain.Enums;
using Uptime.Shared.Enums;

namespace Uptime.Application.Common;

/// <summary>
/// Provides utility methods for mapping between WorkflowPhase (internal state machine) 
/// and WorkflowStatus (external representation).
/// </summary>
public static class WorkflowStatusMapper
{
    /// <summary>
    /// Maps internal WorkflowPhase (used in state machine) to external WorkflowStatus (used in queries/UI).
    /// </summary>
    public static WorkflowStatus MapToWorkflowStatus(this WorkflowPhase phase)
    {
        return phase switch
        {
            WorkflowPhase.Approval => WorkflowStatus.InProgress,
            WorkflowPhase.Signing => WorkflowStatus.InProgress,
            WorkflowPhase.Review => WorkflowStatus.InProgress,
            WorkflowPhase.Rejected => WorkflowStatus.Completed,
            WorkflowPhase.Completed => WorkflowStatus.Completed,
            WorkflowPhase.Cancelled => WorkflowStatus.Completed,
            WorkflowPhase.Terminated => WorkflowStatus.Completed,
            _ => WorkflowStatus.Invalid
        };
    }
}