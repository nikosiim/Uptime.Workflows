using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

/// <summary>
/// Manages the execution of tasks in a workflow replicator, supporting sequential and parallel execution.
/// </summary>
public class Replicator : IReplicator
{
    /// <summary>
    /// Gets or sets the execution type of the replicator (Sequential or Parallel).
    /// </summary>
    public ReplicatorType Type { get; set; } = ReplicatorType.Sequential;

    /// <summary>
    /// The collection of items managed by the replicator.
    /// </summary>
    public List<ReplicatorItem> Items { get; set; } = [];

    /// <summary>
    /// Determines whether all tasks in the replicator have been completed.
    /// </summary>
    public bool IsComplete => Items.All(item => item.Status is ReplicatorItemStatus.Completed or ReplicatorItemStatus.Canceled);

    /// <summary>
    /// Event triggered when a child activity is initialized.
    /// </summary>
    public Action<IWorkflowActivityContext, IWorkflowActivity>? OnChildInitialized { get; set; }
    
    /// <summary>
    /// Event triggered when all tasks in the replicator are completed.
    /// </summary>
    public Func<Task>? OnAllTasksCompleted { get; set; }

    /// <summary>
    /// Factory function for creating workflow activities for each replicator item.
    /// </summary>
    public Func<ReplicatorItem, IWorkflowActivity> ChildActivity { get; set; }
        = _ => throw new InvalidOperationException("No factory set.");

    /// <summary>
    /// Executes the replicator's tasks based on its configured execution type (Sequential or Parallel).
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Items.Count != 0)
        {
            Task executionTask = Type switch
            {
                ReplicatorType.Sequential => ExecuteSequentialAsync(cancellationToken),
                ReplicatorType.Parallel   => ExecuteParallelAsync(cancellationToken),
                _ => throw new NotSupportedException($"Replicator type '{Type}' is not supported.")
            };

            await executionTask;
        }

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
        foreach (ReplicatorItem item in Items.Where(item => item.Status == ReplicatorItemStatus.NotStarted))
        {
            IWorkflowActivity activity = await InitializeActivityAsync(item, cancellationToken);
            if (activity.IsCompleted) // Does not apply to UserActivity, as their response is not immediately apparent
            {
                item.Status = ReplicatorItemStatus.Completed;
            }
            else
            {
                item.Status = ReplicatorItemStatus.InProgress;
                // In a real user-interaction scenario, we stop here and wait.
                return;
            }
        }
    }

    /// <summary>
    /// Executes tasks in parallel, running all tasks concurrently.
    /// </summary>
    private async Task ExecuteParallelAsync(CancellationToken cancellationToken)
    {
        IEnumerable<Task> tasks = Items
            .Where(item => item.Status == ReplicatorItemStatus.NotStarted)
            .Select(async item =>
            {
                item.Status = ReplicatorItemStatus.InProgress;

                IWorkflowActivity activity = await InitializeActivityAsync(item, cancellationToken);
                if (activity.IsCompleted) // Does not apply to UserActivity, as their response is not immediately apparent
                {
                    item.Status = ReplicatorItemStatus.Completed;
                }
            });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Initializes and executes an activity for the given replicator item.
    /// </summary>
    private async Task<IWorkflowActivity> InitializeActivityAsync(ReplicatorItem item, CancellationToken cancellationToken)
    {
        IWorkflowActivity activity = ChildActivity(item);
        OnChildInitialized?.Invoke(item.ActivityContext, activity);
        await activity.ExecuteAsync(cancellationToken);

        return activity;
    }
}