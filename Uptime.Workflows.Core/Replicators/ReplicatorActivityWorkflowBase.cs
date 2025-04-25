using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public abstract class ReplicatorActivityWorkflowBase<TContext>(
    IStateMachineFactory<BaseState, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository,
    ILogger<WorkflowBase<TContext>> logger)
    : ActivityWorkflowBase<TContext>(stateMachineFactory, repository, logger)
    where TContext : class, IReplicatorWorkflowContext, new()
{
    private ReplicatorManager? _replicatorManager;
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;

    protected abstract IReplicatorActivityProvider ActivityProvider { get; }
    
    protected virtual IReplicatorPhaseBuilder CreateReplicatorPhaseBuilder()
    {
        return new ReplicatorPhaseBuilder(WorkflowDefinition.ReplicatorConfiguration!.PhaseConfigurations);
    }
    
    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        IReplicatorPhaseBuilder builder = CreateReplicatorPhaseBuilder();
        List<ReplicatorPhase> replicatorPhases = builder.BuildPhases(payload, WorkflowId);
        
        // Convert phases into replicator states and store them in the workflow context.
        WorkflowContext.ReplicatorStates = replicatorPhases.ToDictionary(
            phase => phase.PhaseName,
            phase => new ReplicatorState
            {
                ReplicatorType = phase.Type,
                Items = phase.TaskData.Select(data => new ReplicatorItem(Guid.NewGuid(), data) ).ToList()
            }
        );

        InitializeReplicatorManagerAsync(cancellationToken);
    }

    protected override string OnWorkflowModification()
    {
        var modificationContext = string.Empty;

        if (!WorkflowContext.ReplicatorStates.TryGetValue(Machine.CurrentState.Value, out ReplicatorState? replicatorState))
        {
            _logger.LogWarning("Workflow {WorkflowId} Replicator phase not found {phaseId}", WorkflowId, Machine.CurrentState.Value);
            return modificationContext;
        }

        if (replicatorState.ReplicatorType == ReplicatorType.Parallel)
        {
            _logger.LogWarning("Workflow {WorkflowId} update not allowed for parallel workflows", WorkflowId);
            return modificationContext;
        }

        if (!replicatorState.HasActiveItems)
        {
            _logger.LogWarning("Workflow {WorkflowId} update not allowed in this phase", WorkflowId);
            return modificationContext;
        }

        return modificationContext;
    }
    
    protected override async Task OnTaskAlteredAsync(WorkflowTaskContext context, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        if (CreateChildActivity(context) is not { } taskActivity)
        {
            throw new InvalidOperationException($"Task {context.TaskId} is not a user-interrupting activity.");
        }

        if (!taskActivity.IsCompleted)
        {
            await taskActivity.ChangedTaskAsync(payload, cancellationToken);
            
            string? phase = WorkflowContext.ReplicatorStates.FindPhase(context.TaskGuid);

            if (taskActivity.IsCompleted && !string.IsNullOrWhiteSpace(phase))
            {
                ActivityProvider.OnChildCompleted(phase, taskActivity, WorkflowContext);

                UpdateWorkflowContextReplicatorState(context.TaskGuid, ReplicatorItemStatus.Completed);

                await RunReplicatorAsync(phase, cancellationToken);
            }
        }
    }
    
    protected virtual UserTaskActivity? CreateChildActivity(WorkflowTaskContext context)
    {
        ReplicatorItem? item = WorkflowContext.ReplicatorStates.FindReplicatorItem(context.TaskGuid);
        if (item != null)
        {
            return ActivityProvider.CreateActivity(context, item.Data) as UserTaskActivity;
        }

        return null;
    }
    
    protected virtual void UpdateWorkflowContextReplicatorState(Guid taskGuid, ReplicatorItemStatus status)
    {
        ReplicatorItem? item = WorkflowContext.ReplicatorStates.FindReplicatorItem(taskGuid);
        if (item != null)
        {
            item.Status = status;
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
            _replicatorManager = new ReplicatorManager(WorkflowId, ActivityProvider, this);
            _replicatorManager.LoadReplicatorsAsync(WorkflowContext.ReplicatorStates, cancellationToken);
        }
    }
}