using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public sealed class ReplicatorManager
{
    private readonly Dictionary<string, Replicator> _replicators = new();

    /// <summary>
    /// Initializes replicators from the workflow context.
    /// </summary>
    public void LoadReplicatorsAsync(
        Dictionary<string, ReplicatorState> states,
        Func<ReplicatorItem, IWorkflowActivity> createActivity,
        Action<string, IWorkflowActivityContext, IWorkflowActivity> onChildInitialized,
        Func<Task> onAllTasksCompleted,
        CancellationToken ct)
    {
        _replicators.Clear();

        foreach ((string phaseId, ReplicatorState state) in states)
        {
            List<ReplicatorItem> activeItems = state.Items
                .Where(i => i.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress)
                .ToList();

            var replicator = new Replicator
            {
                Type = state.ReplicatorType,
                Items = activeItems,
                ChildActivity = createActivity,
                OnChildInitialized = (ctx, act) => onChildInitialized(phaseId, ctx, act),
                OnAllTasksCompleted = onAllTasksCompleted
            };

            _replicators[phaseId] = replicator;
        }
    }

    /// <summary>
    /// Runs all replicators for the specified phase.
    /// </summary>
    public Task RunReplicatorAsync(string phaseName, CancellationToken ct)
    {
        return _replicators.TryGetValue(phaseName, out Replicator? r) 
            ? r.ExecuteAsync(ct) 
            : Task.CompletedTask;
    }
}