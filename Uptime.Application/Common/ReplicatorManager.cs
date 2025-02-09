using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Common;

public class ReplicatorManager<TData>(WorkflowId workflowId, IWorkflowActivityFactory<TData> activityFactory, IWorkflowMachine workflowMachine)
    where TData : IReplicatorItem
{
    private readonly Dictionary<string, Replicator<TData>> _replicators = new();

    /// <summary>
    /// Initializes replicators from the workflow context.
    /// </summary>
    public void LoadReplicators(Dictionary<string, ReplicatorState<TData>> replicatorStates)
    {
        _replicators.Clear();
        
        foreach ((string phase, ReplicatorState<TData> state) in replicatorStates)
        {
            _replicators[phase] = new Replicator<TData>
            {
                Type = state.Type,
                Items = state.Items,

                // See jama tuleb siin korda saada
                ChildActivityFactory = data =>
                {
                    var replicatorItem = state.Items.FirstOrDefault(item => item.Data.Equals(data));
                    Guid taskGuid = replicatorItem?.TaskGuid ?? Guid.NewGuid();

                    return activityFactory.CreateActivity(workflowId, data, taskGuid);
                },

                OnChildInitialized = (data, activity) => activityFactory.OnChildInitialized(phase, data, activity),
                OnChildCompleted = (data, activity) => activityFactory.OnChildCompleted(phase, data, activity),
                OnAllTasksCompleted = async () => await workflowMachine.FireAsync(phase, WorkflowTrigger.AllTasksCompleted)
            };
        }
    }

    /// <summary>
    /// Runs all replicators for the specified phase.
    /// </summary>
    public async Task RunReplicatorAsync(string phaseName)
    {
        if (!_replicators.TryGetValue(phaseName, out Replicator<TData>? replicator))
            return; // No replicator for this phase
        
        await replicator.ExecuteAsync();
    }
}