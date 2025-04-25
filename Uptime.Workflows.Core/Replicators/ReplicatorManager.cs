using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public class ReplicatorManager(WorkflowId workflowId, IReplicatorActivityProvider activityProvider, IWorkflowMachine workflowMachine)
{
    private readonly Dictionary<string, Replicator> _replicators = new();

    /// <summary>
    /// Initializes replicators from the workflow context.
    /// </summary>
    public void LoadReplicatorsAsync(Dictionary<string, ReplicatorState> replicatorStates, CancellationToken cancellationToken)
    {
        _replicators.Clear();
        
        foreach ((string phaseId, ReplicatorState state) in replicatorStates)
        {
            Console.WriteLine($"Initializing replicator for phase '{phaseId}', Task Count: {state.Items.Count}");

            var replicator = new Replicator
            {
                Type = state.ReplicatorType,
                Items = state.Items.Where(item => item.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress).ToList(),
                ChildActivity = item => activityProvider.CreateActivity(new WorkflowTaskContext(workflowId, item.TaskGuid, phaseId), item.Data),
                OnChildInitialized = (data, activity) => activityProvider.OnChildInitialized(phaseId, data, activity),
                OnAllTasksCompleted = async () => await workflowMachine.TriggerTransitionAsync(WorkflowTrigger.AllTasksCompleted, cancellationToken)
            };

            _replicators[phaseId] = replicator;
        }
    }

    /// <summary>
    /// Runs all replicators for the specified phase.
    /// </summary>
    public async Task RunReplicatorAsync(string phaseName, CancellationToken cancellationToken)
    {
        if (!_replicators.TryGetValue(phaseName, out Replicator? replicator))
            return; // No replicator for this phase
        
        await replicator.ExecuteAsync(cancellationToken);
    }
}