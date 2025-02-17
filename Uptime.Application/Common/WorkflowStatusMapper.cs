using Uptime.Application.Workflows.Approval;
using Uptime.Domain.Common;
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
            WorkflowPhase.InProgress => WorkflowStatus.InProgress,
            ApprovalPhase.Approval => WorkflowStatus.InProgress,
            ApprovalPhase.Signing => WorkflowStatus.InProgress,
            WorkflowPhase.Completed => WorkflowStatus.Completed,
            WorkflowPhase.Cancelled => WorkflowStatus.Completed,
            _ => WorkflowStatus.Invalid
        };
    }
}