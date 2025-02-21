using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public class ReplicatorManager(WorkflowId workflowId, IReplicatorActivityProvider activityProvider, IWorkflowMachine workflowMachine)
{
    private readonly Dictionary<string, Replicator> _replicators = new();

    /// <summary>
    /// Initializes replicators from the workflow context.
    /// </summary>
    public void LoadReplicatorsAsync(Dictionary<string, ReplicatorState> replicatorStates, CancellationToken cancellationToken)
    {
        _replicators.Clear();
        
        foreach ((string phase, ReplicatorState state) in replicatorStates)
        {
            Console.WriteLine($"Initializing replicator for phase '{phase}', Task Count: {state.Items.Count}");

            var replicator = new Replicator
            {
                Type = state.Type,
                Items = state.Items.Where(item => item.Status == ReplicatorItemStatus.NotStarted || item.Status == ReplicatorItemStatus.InProgress).ToList(),
                ChildActivity = data => activityProvider.CreateActivity(phase, data, new WorkflowTaskContext(workflowId)),
                OnChildInitialized = (data, activity) => activityProvider.OnChildInitialized(phase, data, activity),
                OnAllTasksCompleted = async () => await workflowMachine.TriggerTransitionAsync(WorkflowTrigger.AllTasksCompleted, cancellationToken)
            };

            _replicators[phase] = replicator;
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