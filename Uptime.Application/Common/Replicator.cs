using Uptime.Application.Enums;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public class Replicator<TItem> : IReplicator<TItem> where TItem : IReplicatorItem
{
    public ReplicatorType Type { get; set; } = ReplicatorType.Sequential;
    public IEnumerable<TItem> Items { get; set; } = [];
    public bool IsItemsCompleted { get; private set; }

    public Action<TItem, IWorkflowActivity>? OnChildInitialized { get; set; }
    public Action<TItem, IWorkflowActivity>? OnChildCompleted { get; set; }
    public Func<TItem, IWorkflowActivity> ChildActivityFactory { get; set; }
        = _ => throw new InvalidOperationException("No factory set.");

    public async Task ExecuteAsync()
    {
        if (!Items.Any())
        {
            IsItemsCompleted = true;
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

        // Determine if all items are completed
        IsItemsCompleted = Items.All(item => item.IsCompleted);
    }

    private async Task ExecuteSequentialAsync()
    {
        foreach (TItem item in Items)
        {
            if (item.IsCompleted)
                continue;

            IWorkflowActivity activity = ChildActivityFactory(item);

            OnChildInitialized?.Invoke(item, activity);

            await activity.ExecuteAsync();

            if (item.IsCompleted)
            {
                OnChildCompleted?.Invoke(item, activity);
            }
            else
            {
                return;
            }
        }
    }

    private async Task ExecuteParallelAsync()
    {
        IEnumerable<Task> tasks = Items
            .Where(item => !item.IsCompleted)
            .Select(async item =>
            {
                IWorkflowActivity activity = ChildActivityFactory(item);

                OnChildInitialized?.Invoke(item, activity);

                await activity.ExecuteAsync();

                if (item.IsCompleted)
                {
                    OnChildCompleted?.Invoke(item, activity);
                }
            });

        await Task.WhenAll(tasks);
    }
}