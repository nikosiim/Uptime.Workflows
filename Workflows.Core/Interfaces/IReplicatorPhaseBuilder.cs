namespace Workflows.Core.Interfaces;

/// <summary>
/// Interface for a builder that constructs all replicator phases (and their tasks) for a given workflow instance.
/// <para>
/// <b>Why do we need this?</b><br/>
/// Workflows define their possible phases and how tasks are generated/configured for each phase.
/// At runtime, when a workflow is activated, we need to build the actual set of phases/tasks using the *current* workflow context data.
/// This interface provides the API for that logic.
/// </para>
/// </summary>
public interface IReplicatorPhaseBuilder
{
    List<ReplicatorPhase> BuildPhases(IWorkflowContext context);
}