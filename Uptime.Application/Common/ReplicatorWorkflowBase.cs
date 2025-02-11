using Microsoft.Extensions.Logging;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public abstract class ReplicatorWorkflowBase<TContext, TData>(
    IWorkflowStateRepository<TContext> stateRepository,
    IWorkflowPersistenceService workflowService, 
    ITaskService taskService, 
    IWorkflowActivityFactory<TData> activityFactory,
    IReplicatorPhaseBuilder<TData> replicatorPhaseBuilder,
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(stateRepository, workflowService, logger)
    where TContext : class, IReplicatorWorkflowContext<TData>, new()
    where TData : IReplicatorItem
{
    private ReplicatorManager<TData>? _replicatorManager;

    protected override void OnWorkflowActivated(IWorkflowPayload payload)
    {
        List<ReplicatorPhase<TData>> replicatorPhases = replicatorPhaseBuilder.BuildPhases(payload, WorkflowId);

        Dictionary<string, ReplicatorState<TData>> replicatorStates = replicatorPhases.ToDictionary(
            phase => phase.PhaseName,
            phase => new ReplicatorState<TData>
            {
                Type = ReplicatorType.Sequential,
                Items = phase.TaskData.Select(data => new ReplicatorItem<TData> { Data = data }).ToList()
            });

        WorkflowContext.ReplicatorStates = replicatorStates;
        EnsureReplicatorManagerInitialized();
    }

    protected async Task RunReplicatorAsync(string phaseName)
    {
        EnsureReplicatorManagerInitialized();
        await _replicatorManager!.RunReplicatorAsync(phaseName);
    }

    protected UserTaskActivity? CreateChildActivity(WorkflowTaskContext context)
    {
        TData? taskData = GetTaskDataForContext(context);
        return taskData != null ? activityFactory.CreateActivity(taskData, context) as UserTaskActivity : null;
    }

    protected override async Task<WorkflowPhase> AlterTaskInternalAsync(IAlterTaskPayload payload)
    {
        WorkflowTaskContext? context = await taskService.GetWorkflowTaskContextAsync(payload.TaskId);
        if (context == null) return Machine.CurrentState;

        if (CreateChildActivity(context) is not { } taskActivity)
        {
            throw new InvalidOperationException($"Task {payload.TaskId} is not a user-interrupting activity.");
        }

        await taskActivity.OnTaskChanged(payload);
        UpdateReplicatorState(context.TaskGuid, taskActivity.IsCompleted);

        if (taskActivity.IsCompleted)
        {
            string? phaseName = WorkflowContext.ReplicatorStates
                .FirstOrDefault(kvp => kvp.Value.Items.Any(item => item.TaskGuid == context.TaskGuid)).Key;

            if (phaseName != null)
            {
                await RunReplicatorAsync(phaseName);
            }
        }

        return await SaveWorkflowStateAsync();
    }

    protected override void UpdateReplicatorState(Guid taskGuid, bool isCompleted)
    {
        foreach (ReplicatorState<TData> state in WorkflowContext.ReplicatorStates.Values)
        {
            foreach (ReplicatorItem<TData> item in state.Items.Where(t => t.TaskGuid == taskGuid))
            {
                item.IsCompleted = isCompleted;
                return;
            }
        }
    }
    
    private TData? GetTaskDataForContext(WorkflowTaskContext context)
    {
        ReplicatorItem<TData>? item = WorkflowContext.ReplicatorStates
            .SelectMany(kvp => kvp.Value.Items)
            .FirstOrDefault(item => item.TaskGuid == context.TaskGuid);

        return item != null ? item.Data : default;
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