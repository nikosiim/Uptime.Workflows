using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core
{
    /// <summary>
    /// Describes how to generate tasks/items for a single replicator phase (e.g., "Approval", "Signing").
    /// <para>
    /// <b>Key Properties:</b><br/>
    /// - <see cref="ReplicatorType"/>: Delegate that returns the type (Parallel, Sequential, etc.) to use for this phase, based on the workflow context.
    /// - <see cref="ActivityData"/>: Delegate that returns the set of task "input data" for this phase — e.g., a list of ApprovalTaskData, one for each assignee.
    /// </para>
    /// <para>
    /// <b>How does this work?</b><br/>
    /// When the workflow starts or rehydrates, these delegates are called to generate the runtime configuration
    /// for this phase, producing the correct number of tasks with the right data and assignments.
    /// </para>
    /// <para>
    /// <b>How to use:</b><br/>
    /// Typically, you provide these delegates in your workflow definition (e.g., ApprovalWorkflowDefinition),
    /// so each phase can have custom task generation logic.
    /// </para>
    /// </summary>
    public class ReplicatorPhaseConfiguration
    {
        public required Func<IWorkflowContext, ReplicatorType> ReplicatorType { get; init; }
        public required Func<IWorkflowContext, IEnumerable<object>> ActivityData { get; init; }
    }
}