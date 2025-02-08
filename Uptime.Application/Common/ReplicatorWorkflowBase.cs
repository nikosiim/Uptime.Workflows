using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Shared.Enums;

namespace Uptime.Application.Common;

/// <summary>
/// Base class for workflows that use one or more replicators.
/// Handles replicator execution, task lifecycle management, and workflow state updates.
/// </summary>
/// <typeparam name="TContext">The workflow context type, implementing <see cref="IWorkflowContext"/>.</typeparam>
/// <typeparam name="TData">The data type managed by the replicator, implementing <see cref="IReplicatorItem"/>.</typeparam>
public abstract class ReplicatorWorkflowBase<TContext, TData>(IWorkflowService workflowService, ITaskService taskService) : WorkflowBase<TContext>(workflowService)
    where TContext : IWorkflowContext, new() where TData : IReplicatorItem
{
    /// <summary>
    /// Gets the task service used for handling workflow tasks.
    /// </summary>
    protected ITaskService TaskService => taskService;

    /// <summary>
    /// Retrieves the list of replicator states associated with the workflow.
    /// </summary>
    /// <returns>A list of <see cref="ReplicatorState{TData}"/> instances representing the state of each replicator.</returns>
    protected abstract List<ReplicatorState<TData>> GetReplicatorStates();

    /// <summary>
    /// Handles user-driven task alterations in workflows that contain user-interrupting tasks.
    /// </summary>
    /// <param name="payload">The payload containing task modification details.</param>
    /// <returns>The updated workflow status after processing the task alteration.</returns>
    public async Task<WorkflowStatus> AlterTaskAsync(AlterTaskPayload payload)
    {
        WorkflowTaskContext? context = await taskService.GetWorkflowTaskContextAsync(payload.TaskId);
        if (context == null) return Machine.State;

        if (CreateChildActivity(context) is not { } taskActivity)
        {
            throw new InvalidOperationException($"Task {payload.TaskId} is not a user-interrupting activity.");
        }

        await taskActivity.OnTaskChanged(payload);
        UpdateReplicatorState(context.TaskGuid, taskActivity.IsCompleted);
        await RunReplicatorsAsync();

        return await CommitWorkflowStateAsync();
    }

    /// <summary>
    /// Executes all replicators in the workflow, ensuring task execution and state transitions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task RunReplicatorsAsync()
    {
        foreach (ReplicatorState<TData> replicatorState in GetReplicatorStates())
        {
            var replicator = new Replicator<TData>
            {
                Type = replicatorState.Type,
                Items = replicatorState.Items,
                ChildActivityFactory = CreateChildActivity,
                OnChildInitialized = OnReplicatorChildInitialized,
                OnChildCompleted = OnReplicatorChildCompleted,
                OnAllTasksCompleted = async () => await OnAllTasksCompleted(replicatorState)
            };

            await replicator.ExecuteAsync();
        }
    }

    /// <summary>
    /// Updates the state of a replicator when a task is completed.
    /// </summary>
    /// <param name="taskGuid">The unique identifier of the completed task.</param>
    /// <param name="isCompleted">A boolean indicating whether the task is completed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract void UpdateReplicatorState(Guid taskGuid, bool isCompleted);

    /// <summary>
    /// Creates a child activity for the given replicator data item.
    /// </summary>
    /// <param name="data">The replicator item data.</param>
    /// <returns>An instance of <see cref="IWorkflowActivity"/> representing the created activity.</returns>
    protected abstract IWorkflowActivity CreateChildActivity(TData data);

    /// <summary>
    /// Creates a user-driven task activity for a given workflow task context.
    /// </summary>
    /// <param name="context">The task context containing task details.</param>
    /// <returns>An instance of <see cref="UserTaskActivity"/> representing the created activity.</returns>
    protected abstract UserTaskActivity CreateChildActivity(WorkflowTaskContext context);

    /// <summary>
    /// Invoked when a child activity is initialized in a replicator.
    /// This method can be overridden to modify task context before execution.
    /// </summary>
    /// <param name="data">The replicator item data.</param>
    /// <param name="activity">The initialized workflow activity.</param>
    protected virtual void OnReplicatorChildInitialized(TData data, IWorkflowActivity activity) { }

    /// <summary>
    /// Invoked when a child activity is completed in a replicator.
    /// This method can be overridden to process any post-task completion logic.
    /// </summary>
    /// <param name="data">The replicator item data.</param>
    /// <param name="activity">The completed workflow activity.</param>
    protected virtual void OnReplicatorChildCompleted(TData data, IWorkflowActivity activity) { }

    /// <summary>
    /// Invoked when all tasks within a replicator have been completed.
    /// </summary>
    /// <param name="replicatorState">The state of the completed replicator.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task OnAllTasksCompleted(ReplicatorState<TData> replicatorState)
    {
        await FireAsync(WorkflowTrigger.AllTasksCompleted);
    }
}