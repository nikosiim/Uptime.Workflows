using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core;

public static class WorkflowPhaseExtensions
{
    /// <summary>
    /// Determines whether the workflow phase represents a terminal state.
    /// </summary>
    /// <param name="phase">The workflow phase.</param>
    /// <returns><c>true</c> if the phase is Completed, Cancelled; otherwise, <c>false</c>.</returns>
    public static bool IsFinal(this BaseState phase)
    {
        return phase == BaseState.Completed || phase ==  BaseState.Cancelled;
    }
}