using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class ReplicatorActivityWorkflowBase<TContext>(
    IStateMachineFactory<BaseState, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository,
    ILogger<WorkflowBase<TContext>> logger)
    : ActivityWorkflowBase<TContext>(stateMachineFactory, repository, logger), IReplicatorActivityWorkflowMachine
    where TContext : class, IReplicatorWorkflowContext, new()
{
    private ReplicatorManager? _replicatorManager;

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
                Items = phase.TaskData.Select(data => new ReplicatorItem { Data = data }).ToList()
            }
        );

        InitializeReplicatorManagerAsync(cancellationToken);
    }
    
    protected override async Task OnTaskChangedAsync(WorkflowTaskContext storedTaskContext, Dictionary<string, string?> alterTaskPayload, CancellationToken cancellationToken)
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
                ActivityProvider.OnChildCompleted(phase, taskActivity, WorkflowContext);

                UpdateWorkflowContextReplicatorState(storedTaskContext.TaskGuid, ReplicatorItemStatus.Completed);

                await RunReplicatorAsync(phase, cancellationToken);
            }
        }
    }
    
    public async Task<string> ModifyWorkflow(ModificationContext modificationContext, CancellationToken cancellationToken)
    {
        bool isModified = await OnWorkflowModifiedAsync(modificationContext, cancellationToken);
        if (isModified)
        {
            await SaveWorkflowStateAsync(cancellationToken);
        }

        return Machine.CurrentState.Value;
    }

    protected virtual Task<bool> OnWorkflowModifiedAsync(ModificationContext modificationContext, CancellationToken cancellationToken)
    {
        if (WorkflowId.Value != modificationContext.WorkflowId || !WorkflowContext.ReplicatorStates.ContainsKey(modificationContext.PhaseId))
        {
            throw new InvalidOperationException("Modification context does not match the current workflow state.");
        }

        return Task.FromResult(false);
    }
    
    protected virtual UserTaskActivity? CreateChildActivity(WorkflowTaskContext context)
    {
        ReplicatorItem? item = WorkflowContext.ReplicatorStates.FindReplicatorItem(context.TaskGuid, out string? phase);
        if (item != null)
        {
            return ActivityProvider.CreateActivity(phase!, item.Data, context) as UserTaskActivity;
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