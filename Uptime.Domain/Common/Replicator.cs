using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

/// <summary>
/// Manages the execution of tasks in a workflow replicator, supporting sequential and parallel execution.
/// </summary>
/// <typeparam name="TItem">The type of data associated with each task.</typeparam>
public class Replicator<TItem> : IReplicator<TItem>
{
    /// <summary>
    /// Gets or sets the execution type of the replicator (Sequential or Parallel).
    /// </summary>
    public ReplicatorType Type { get; set; } = ReplicatorType.Sequential;

    /// <summary>
    /// The collection of items managed by the replicator.
    /// </summary>
    public List<ReplicatorItem<TItem>> Items { get; set; } = [];

    /// <summary>
    /// Determines whether all tasks in the replicator have been completed.
    /// </summary>
    public bool IsComplete => Items.All(item => item.IsCompleted);

    /// <summary>
    /// Event triggered when a child activity is initialized.
    /// </summary>
    public Action<TItem, IWorkflowActivity>? OnChildInitialized { get; set; }

    /// <summary>
    /// Event triggered when a child activity is completed.
    /// </summary>
    public Action<TItem, IWorkflowActivity>? OnChildCompleted { get; set; }

    /// <summary>
    /// Event triggered when all tasks in the replicator are completed.
    /// </summary>
    public Func<Task>? OnAllTasksCompleted { get; set; }

    /// <summary>
    /// Factory function for creating workflow activities for each replicator item.
    /// </summary>
    public Func<TItem, IWorkflowActivity> ChildActivityFactory { get; set; }
        = _ => throw new InvalidOperationException("No factory set.");

    /// <summary>
    /// Executes the replicator's tasks based on its configured execution type (Sequential or Parallel).
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!Items.Any())
        {
            if (OnAllTasksCompleted != null)
            {
                await OnAllTasksCompleted.Invoke();
            }
            return;
        }

        Task executionTask = Type switch
        {
            ReplicatorType.Sequential => ExecuteSequentialAsync(cancellationToken),
            ReplicatorType.Parallel => ExecuteParallelAsync(cancellationToken),
            _ => throw new NotSupportedException($"Replicator type '{Type}' is not supported.")
        };

        await executionTask;

        if (IsComplete && OnAllTasksCompleted != null)
        {
            await OnAllTasksCompleted.Invoke();
        }
    }

    /// <summary>
    /// Executes tasks sequentially, processing one task at a time.
    /// If a task is incomplete, execution halts until it is completed.
    /// </summary>
    private async Task ExecuteSequentialAsync(CancellationToken cancellationToken)
    {
        foreach (ReplicatorItem<TItem> item in Items.Where(item => !item.IsCompleted))
        {
            IWorkflowActivity activity = await InitializeActivityAsync(item, cancellationToken);

            if (activity.IsCompleted)
            {
                item.IsCompleted = true;
                OnChildCompleted?.Invoke(item.Data, activity);
            }
            else
            {
                return; // Stop execution, waiting for user action
            }
        }
    }

    /// <summary>
    /// Executes tasks in parallel, running all tasks concurrently.
    /// </summary>
    private async Task ExecuteParallelAsync(CancellationToken cancellationToken)
    {
        IEnumerable<Task> tasks = Items
            .Where(item => !item.IsCompleted)
            .Select(async item =>
            {
                IWorkflowActivity activity = await InitializeActivityAsync(item, cancellationToken);
                if (activity.IsCompleted)
                {
                    item.IsCompleted = true;
                    OnChildCompleted?.Invoke(item.Data, activity);
                }
            });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Initializes and executes an activity for the given replicator item.
    /// </summary>
    private async Task<IWorkflowActivity> InitializeActivityAsync(ReplicatorItem<TItem> item, CancellationToken cancellationToken)
    {
        IWorkflowActivity activity = ChildActivityFactory(item.Data);
        OnChildInitialized?.Invoke(item.Data, activity);
        await activity.ExecuteAsync(cancellationToken);

        if (activity is UserTaskActivity userTaskActivity)
        {
            item.TaskGuid = userTaskActivity.Context.TaskGuid;
        }

        return activity;
    }
}
