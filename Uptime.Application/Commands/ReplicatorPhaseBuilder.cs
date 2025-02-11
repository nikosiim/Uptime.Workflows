using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Commands;

/// <summary>
/// A default implementation of <see cref="IReplicatorPhaseBuilder{TData}"/> that builds replicator phases
/// based on configured phase names and associated functions to extract task data from the payload.
/// </summary>
/// <typeparam name="TData">The type of data for each replicator phase.</typeparam>
public class ReplicatorPhaseBuilder<TData> : IReplicatorPhaseBuilder<TData>
{
    // Dictionary mapping phase names to functions that extract task data.
    private readonly Dictionary<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<TData>>> _phaseConfigurations;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplicatorPhaseBuilder{TData}"/> class.
    /// </summary>
    /// <param name="phaseConfigurations">
    /// A dictionary where the key is the phase name and the value is a delegate that extracts task data from the payload.
    /// </param>
    public ReplicatorPhaseBuilder(Dictionary<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<TData>>> phaseConfigurations)
    {
        _phaseConfigurations = phaseConfigurations;
    }

    /// <summary>
    /// Builds the replicator phases using the configured phase names and task data extraction functions.
    /// </summary>
    /// <param name="payload">The workflow payload containing task data.</param>
    /// <param name="workflowId">The workflow ID used for retrieving phase-specific data.</param>
    /// <returns>A list of <see cref="ReplicatorPhase{TData}"/> instances.</returns>
    public List<ReplicatorPhase<TData>> BuildPhases(IWorkflowPayload payload, WorkflowId workflowId)
    {
        var phases = new List<ReplicatorPhase<TData>>();
        foreach (KeyValuePair<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<TData>>> kvp in _phaseConfigurations)
        {
            // Create a replicator phase for each configured phase.
            var phase = new ReplicatorPhase<TData>
            {
                PhaseName = kvp.Key,
                TaskData = kvp.Value(payload, workflowId).ToList()
            };
            phases.Add(phase);
        }
        return phases;
    }
}