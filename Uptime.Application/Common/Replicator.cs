using Uptime.Application.Enums;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public class Replicator<TItem> : IReplicator<TItem>
{
    public ReplicatorType Type { get; set; } = ReplicatorType.Sequential;
    public List<ReplicatorItem<TItem>> Items { get; set; } = [];
    public bool IsComplete => Items.All(item => item.IsCompleted);

    public Action<TItem, IWorkflowActivity>? OnChildInitialized { get; set; }
    public Action<TItem, IWorkflowActivity>? OnChildCompleted { get; set; }
    public Func<Task>? OnAllTasksCompleted { get; set; }
    public Func<TItem, IWorkflowActivity> ChildActivityFactory { get; set; }
        = _ => throw new InvalidOperationException("No factory set.");

    public async Task ExecuteAsync()
    {
        if (Items.Count == 0)
        {
            if (OnAllTasksCompleted != null)
            {
                await OnAllTasksCompleted.Invoke();
            }
            return;
        }

        switch (Type)
        {
            case ReplicatorType.Sequential:
                await ExecuteSequentialAsync();
                break;
            case ReplicatorType.Parallel:
                await ExecuteParallelAsync();
                break;
            default:
                throw new NotSupportedException($"Replicator type '{Type}' is not supported.");
        }

        if (IsComplete && OnAllTasksCompleted != null)
        {
            await OnAllTasksCompleted.Invoke();
        }
    }

    private async Task ExecuteSequentialAsync()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            ReplicatorItem<TItem> item = Items[i];

            if (item.IsCompleted)
                continue;

            IWorkflowActivity activity = ChildActivityFactory(item.Data);
            OnChildInitialized?.Invoke(item.Data, activity);
            await activity.ExecuteAsync();

            if (activity.IsCompleted)
            {
                OnChildCompleted?.Invoke(item.Data, activity);
                Items[i].IsCompleted = true; // Mark as completed
            }
            else
            {
                return; // Stop execution, waiting for user action
            }
        }
    }

    private async Task ExecuteParallelAsync()
    {
        var tasks = new List<Task>();

        for (var i = 0; i < Items.Count; i++)
        {
            ReplicatorItem<TItem> item = Items[i];

            if (item.IsCompleted)
                continue;

            int j = i;
            tasks.Add(Task.Run(async () =>
            {
                IWorkflowActivity activity = ChildActivityFactory(item.Data);
                OnChildInitialized?.Invoke(item.Data, activity);
                await activity.ExecuteAsync();

                if (activity.IsCompleted)
                {
                    OnChildCompleted?.Invoke(item.Data, activity);
                    Items[j].IsCompleted = true; // Mark as completed
                }
            }));
        }

        await Task.WhenAll(tasks);

        if (IsComplete)
        {
            OnAllTasksCompleted?.Invoke();
        }
    }
}