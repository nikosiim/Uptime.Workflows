using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class ReplicatorActivityWorkflowBase<TContext>(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository, 
    IReplicatorActivityProvider activityFactory,
    IReplicatorPhaseBuilder replicatorPhaseBuilder,
    ILogger<WorkflowBase<TContext>> logger)
    : ActivityWorkflowBase<TContext>(stateMachineFactory, repository, logger)
    where TContext : class, IReplicatorWorkflowContext, new()
{
    private ReplicatorManager? _replicatorManager;

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        List<ReplicatorPhase> replicatorPhases = replicatorPhaseBuilder.BuildPhases(payload, WorkflowId);

        // Convert phases into replicator states and store them in the workflow context.
        WorkflowContext.ReplicatorStates = replicatorPhases.ToDictionary(
            phase => phase.PhaseName,
            phase => new ReplicatorState
            {
                Type = phase.Type,
                Items = phase.TaskData.Select(data => new ReplicatorItem { Data = data }).ToList()
            }
        );

        InitializeReplicatorManagerAsync(cancellationToken);
    }
    
    protected override async Task AlterTaskInternalAsync(WorkflowTaskContext storedTaskContext, Dictionary<string, string?> alterTaskPayload, CancellationToken cancellationToken)
    {
        if (CreateChildActivity(storedTaskContext) is not { } taskActivity)
        {
            throw new InvalidOperationException($"Task {storedTaskContext.TaskId} is not a user-interrupting activity.");
        }

        if (!taskActivity.IsCompleted)
        {
            await taskActivity.ChangedTaskAsync(alterTaskPayload, cancellationToken);
            
            string? phase = WorkflowContext.ReplicatorStates.FindPhase(storedTaskContext.TaskGuid);

            if (taskActivity.IsCompleted && !string.IsNullOrWhiteSpace(phase))
            {
                activityFactory.OnChildCompleted(phase, taskActivity, WorkflowContext);

                UpdateWorkflowContextReplicatorState(storedTaskContext.TaskGuid, true);

                await RunReplicatorAsync(phase, cancellationToken);
            }
        }
    }

    protected virtual UserTaskActivity? CreateChildActivity(WorkflowTaskContext context)
    {
        ReplicatorItem? item = WorkflowContext.ReplicatorStates.FindReplicatorItem(context.TaskGuid, out string? phase);
        if (item != null)
        {
            return activityFactory.CreateActivity(phase!, item.Data, context) as UserTaskActivity;
        }

        return null;
    }
    
    protected virtual void UpdateWorkflowContextReplicatorState(Guid taskGuid, bool isCompleted)
    {
        ReplicatorItem? item = WorkflowContext.ReplicatorStates.FindReplicatorItem(taskGuid);
        if (item != null)
        {
            item.IsCompleted = isCompleted;
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
            _replicatorManager = new ReplicatorManager(WorkflowId, activityFactory, this);
            _replicatorManager.LoadReplicatorsAsync(WorkflowContext.ReplicatorStates, cancellationToken);
        }
    }
}