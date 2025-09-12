using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

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
            List<ReplicatorItem> activeItems = state.Items
                .Where(i => i.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress)
                .ToList();

            var replicator = new Replicator
            {
                Type = state.ReplicatorType,
                Items = activeItems,
                ChildActivity = CreateChildActivity,
                OnChildInitialized = (data, activity) => activityProvider.OnChildInitialized(phaseId, data, activity),
                OnAllTasksCompleted = () => workflowMachine.TriggerTransitionAsync(WorkflowTrigger.AllTasksCompleted, cancellationToken)
            };

            _replicators[phaseId] = replicator;
            continue;

            // Local factory method keeps lambdas short & readable
            IWorkflowActivity CreateChildActivity(ReplicatorItem item) =>
                activityProvider.CreateActivity(new WorkflowTaskContext
                {
                    WorkflowId = workflowId,
                    TaskGuid = item.TaskGuid,
                    PhaseId = phaseId
                }, item.Data);
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