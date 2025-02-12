using Uptime.Domain.Enums;

namespace Uptime.Domain.Common;
public static class WorkflowPhaseExtensions
{
    /// <summary>
    /// Determines whether the workflow phase represents a terminal state.
    /// </summary>
    /// <param name="phase">The workflow phase.</param>
    /// <returns><c>true</c> if the phase is Completed, Cancelled, or Terminated; otherwise, <c>false</c>.</returns>
    public static bool IsFinal(this WorkflowPhase phase)
    {
        return phase is WorkflowPhase.Completed or WorkflowPhase.Cancelled or WorkflowPhase.Terminated;
    }
}