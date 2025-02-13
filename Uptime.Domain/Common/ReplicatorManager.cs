using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public class ReplicatorManager<TData>(WorkflowId workflowId, IWorkflowActivityFactory<TData> activityFactory, IWorkflowMachine workflowMachine)
    where TData : IReplicatorItem
{
    private readonly Dictionary<string, Replicator<TData>> _replicators = new();

    /// <summary>
    /// Initializes replicators from the workflow context.
    /// </summary>
    public void LoadReplicatorsAsync(Dictionary<string, ReplicatorState<TData>> replicatorStates, CancellationToken cancellationToken)
    {
        _replicators.Clear();
        
        foreach ((string phase, ReplicatorState<TData> state) in replicatorStates)
        {
            Console.WriteLine($"Initializing replicator for phase '{phase}', Task Count: {state.Items.Count}");

            var replicator = new Replicator<TData>
            {
                Type = state.Type,
                Items = state.Items,
                ChildActivityFactory = data => activityFactory.CreateActivity(phase, data, new WorkflowTaskContext(workflowId)),
                OnChildInitialized = (data, activity) => activityFactory.OnChildInitialized(phase, data, activity),
                OnChildCompleted = (data, activity) => activityFactory.OnChildCompleted(phase, data, activity),
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
        if (!_replicators.TryGetValue(phaseName, out Replicator<TData>? replicator))
            return; // No replicator for this phase
        
        await replicator.ExecuteAsync(cancellationToken);
    }
}