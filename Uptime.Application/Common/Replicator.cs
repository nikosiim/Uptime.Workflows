using Uptime.Application.Enums;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public class Replicator<TItem> : IReplicator<TItem>
{
    public ReplicatorType Type { get; set; } = ReplicatorType.Sequential;
    public List<(TItem Data, Guid TaskGuid, bool IsCompleted)> Items { get; set; } = [];
    public bool IsComplete { get; private set; }

    public Action<TItem, IWorkflowActivity>? OnChildInitialized { get; set; }
    public Action<TItem, IWorkflowActivity>? OnChildCompleted { get; set; }
    public Func<TItem, IWorkflowActivity> ChildActivityFactory { get; set; }
        = _ => throw new InvalidOperationException("No factory set.");

    public async Task ExecuteAsync()
    {
        if (!Items.Any())
        {
            IsComplete = true;
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

        // Check if all tasks are marked as complete
        IsComplete = Items.All(item => item.IsCompleted);
    }

    private async Task ExecuteSequentialAsync()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            (TItem data, Guid taskGuid, bool isCompleted) = Items[i];

            if (isCompleted)
                continue;

            IWorkflowActivity activity = ChildActivityFactory(data);

            OnChildInitialized?.Invoke(data, activity);
            await activity.ExecuteAsync();

            if (activity.IsCompleted)
            {
                OnChildCompleted?.Invoke(data, activity);

                // Update the completion status in ReplicatorState
                Items[i] = (data, taskGuid, true);
            }
            else
            {
                return; // Stop execution since sequential execution requires waiting for completion
            }
        }
    }


    private async Task ExecuteParallelAsync()
    {
        var tasks = new List<Task>();

        for (var i = 0; i < Items.Count; i++)
        {
            (TItem data, Guid taskGuid, bool isCompleted) = Items[i];

            if (isCompleted)
                continue; // Skip already completed tasks

            int  j = i;
            tasks.Add(Task.Run(async () =>
            {
                IWorkflowActivity activity = ChildActivityFactory(data);

                OnChildInitialized?.Invoke(data, activity);
                await activity.ExecuteAsync();

                if (activity.IsCompleted)
                {
                    OnChildCompleted?.Invoke(data, activity);

                    // Update completion status in ReplicatorState
                    Items[j] = (data, taskGuid, true);
                }
            }));
        }

        await Task.WhenAll(tasks); // Wait for all parallel tasks to complete
    }

}
