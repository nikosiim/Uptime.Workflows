using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class ReplicatorWorkflowBase<TContext, TData>(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowStateRepository<TContext> stateRepository,
    IWorkflowActivityFactory<TData> activityFactory,
    IReplicatorPhaseBuilder<TData> replicatorPhaseBuilder,
    ILogger<WorkflowBase<TContext>> logger)
    : ActivityWorkflowBase<TContext>(stateMachineFactory, stateRepository, logger)
    where TContext : class, IReplicatorWorkflowContext<TData>, new()
    where TData : IReplicatorItem
{
    private ReplicatorManager<TData>? _replicatorManager;

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
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

        InitializeReplicatorManagerAsync(cancellationToken);
    }
    
    protected override async Task AlterTaskInternalAsync(WorkflowTaskContext context, CancellationToken cancellationToken)
    {
        if (CreateChildActivity(context) is not { } taskActivity)
        {
            throw new InvalidOperationException($"Task {context.TaskId} is not a user-interrupting activity.");
        }

        await taskActivity.OnTaskChangedAsync(context.Storage, cancellationToken);

        UpdateReplicatorState(context.TaskGuid, taskActivity.IsCompleted);

        if (taskActivity.IsCompleted)
        {
            string? phaseName = WorkflowContext.ReplicatorStates.FirstOrDefault(kvp => kvp.Value.Items.Any(item => item.TaskGuid == context.TaskGuid)).Key;
            if (phaseName != null)
            {
                await RunReplicatorAsync(phaseName, cancellationToken);
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

    protected async Task RunReplicatorAsync(string phaseName, CancellationToken cancellationToken)
    {
        InitializeReplicatorManagerAsync(cancellationToken);
        await _replicatorManager!.RunReplicatorAsync(phaseName, cancellationToken);
    }

    private void InitializeReplicatorManagerAsync(CancellationToken cancellationToken)
    {
        if (_replicatorManager == null)
        {
            _replicatorManager = new ReplicatorManager<TData>(WorkflowId, activityFactory, this);
            _replicatorManager.LoadReplicatorsAsync(WorkflowContext.ReplicatorStates, cancellationToken);
        }
    }
}