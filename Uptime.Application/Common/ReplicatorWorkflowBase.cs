using Microsoft.Extensions.Logging;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public abstract class ReplicatorWorkflowBase<TContext, TData>(
    IWorkflowStateRepository<TContext> stateRepository,
    IWorkflowActivityFactory<TData> activityFactory,
    IReplicatorPhaseBuilder<TData> replicatorPhaseBuilder,
    ILogger<WorkflowBase<TContext>> logger)
    : ActivityWorkflowBase<TContext>(stateRepository, logger)
    where TContext : class, IReplicatorWorkflowContext<TData>, new()
    where TData : IReplicatorItem
{
    private ReplicatorManager<TData>? _replicatorManager;

    protected override void OnWorkflowActivated(IWorkflowPayload payload)
    {
        List<ReplicatorPhase<TData>> replicatorPhases = replicatorPhaseBuilder.BuildPhases(payload, WorkflowId);

        // Convert phases into replicator states and store them in the workflow context.
        WorkflowContext.ReplicatorStates = replicatorPhases.ToDictionary(
            phase => phase.PhaseName,
            phase => new ReplicatorState<TData>
            {
                Type = ReplicatorType.Sequential,
                Items = phase.TaskData.Select(data => new ReplicatorItem<TData> { Data = data }).ToList()
            }
        );

        EnsureReplicatorManagerInitialized();
    }

    protected async Task RunReplicatorAsync(string phaseName)
    {
        EnsureReplicatorManagerInitialized();
        await _replicatorManager!.RunReplicatorAsync(phaseName);
    }
    
    protected override async Task AlterTaskInternalAsync(WorkflowTaskContext context)
    {
        if (CreateChildActivity(context) is not { } taskActivity)
        {
            throw new InvalidOperationException($"Task {context.TaskId} is not a user-interrupting activity.");
        }

        await taskActivity.OnTaskChanged(context.Storage);

        UpdateReplicatorState(context.TaskGuid, taskActivity.IsCompleted);

        if (taskActivity.IsCompleted)
        {
            string? phaseName = WorkflowContext.ReplicatorStates.FirstOrDefault(kvp => kvp.Value.Items.Any(item => item.TaskGuid == context.TaskGuid)).Key;
            if (phaseName != null)
            {
                await RunReplicatorAsync(phaseName);
            }
        }
    }
    
    protected virtual UserTaskActivity? CreateChildActivity(WorkflowTaskContext context)
    {
        // Locate the replicator item matching the task GUID.
        ReplicatorItem<TData>? item = WorkflowContext.ReplicatorStates
            .SelectMany(kvp => kvp.Value.Items)
            .FirstOrDefault(item => item.TaskGuid == context.TaskGuid);

        if (item == null)
            return null;

        return activityFactory.CreateActivity(item.Data, context) as UserTaskActivity;
    }
    
    protected virtual void UpdateReplicatorState(Guid taskGuid, bool isCompleted)
    {
        foreach (ReplicatorState<TData> state in WorkflowContext.ReplicatorStates.Values)
        {
            foreach (ReplicatorItem<TData> item in state.Items.Where(item => item.TaskGuid == taskGuid))
            {
                item.IsCompleted = isCompleted;
                return;
            }
        }
    }
    
    private void EnsureReplicatorManagerInitialized()
    {
        if (_replicatorManager == null)
        {
            _replicatorManager = new ReplicatorManager<TData>(WorkflowId, activityFactory, this);
            _replicatorManager.LoadReplicators(WorkflowContext.ReplicatorStates);
        }
    }
}