using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class ReplicatorWorkflowBase<TContext>(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository, 
    IWorkflowActivityFactory activityFactory,
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
    
    protected override async Task AlterTaskInternalAsync(WorkflowTaskContext context, CancellationToken cancellationToken)
    {
        if (CreateChildActivity(context) is not { } taskActivity)
        {
            throw new InvalidOperationException($"Task {context.TaskId} is not a user-interrupting activity.");
        }

        await taskActivity.OnTaskChangedAsync(context.Storage, cancellationToken);

        UpdateWorkflowContextReplicatorState(context.TaskGuid, taskActivity.IsCompleted);

        // When a task is completed, force the workflow to move forward
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
        KeyValuePair<string, ReplicatorState> phaseEntry = WorkflowContext.ReplicatorStates
            .FirstOrDefault(kvp => kvp.Value.Items.Any(item => item.TaskGuid == context.TaskGuid));
        if (phaseEntry.Key == null)
            return null; // Task not found in any phase

        ReplicatorItem? item = phaseEntry.Value.Items.FirstOrDefault(i => i.TaskGuid == context.TaskGuid);
        if (item == null)
            return null;

        return activityFactory.CreateActivity(phaseEntry.Key, item.Data, context) as UserTaskActivity;
    }
    
    protected virtual void UpdateWorkflowContextReplicatorState(Guid taskGuid, bool isCompleted)
    {
        foreach (ReplicatorState state in WorkflowContext.ReplicatorStates.Values)
        {
            foreach (ReplicatorItem item in state.Items.Where(item => item.TaskGuid == taskGuid))
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
            _replicatorManager = new ReplicatorManager(WorkflowId, activityFactory, this);
            _replicatorManager.LoadReplicatorsAsync(WorkflowContext.ReplicatorStates, cancellationToken);
        }
    }
}