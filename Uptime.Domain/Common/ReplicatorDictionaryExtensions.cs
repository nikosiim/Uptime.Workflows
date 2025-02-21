using Uptime.Domain.Enums;

namespace Uptime.Domain.Common; 

public static class ReplicatorDictionaryExtensions
{
    public static void CancelAllItems(this IDictionary<string, ReplicatorState> replicatorStates, string? phaseName = null)
    {
        if (phaseName is not null)
        {
            if (replicatorStates.TryGetValue(phaseName, out ReplicatorState? state))
            {
                foreach (ReplicatorItem item in state.Items.Where(item => item.Status == ReplicatorItemStatus.NotStarted))
                {
                    item.Status = ReplicatorItemStatus.Canceled;
                }
            }
        }
        else
        {
            // If no specific phase provided, cancel all items in all phases
            foreach (ReplicatorState state in replicatorStates.Values)
            {
                foreach (ReplicatorItem item in state.Items.Where(item => item.Status == ReplicatorItemStatus.NotStarted))
                {
                    item.Status = ReplicatorItemStatus.Canceled;
                }
            }
        }
    }

    public static string? FindPhase(this IReadOnlyDictionary<string, ReplicatorState> states, Guid taskGuid)
    {
        return states.FirstOrDefault(kvp => kvp.Value.HasTaskGuid(taskGuid)).Key;
    }

    public static ReplicatorItem? FindReplicatorItem(this IReadOnlyDictionary<string, ReplicatorState> states, Guid taskGuid)
    {
        foreach (ReplicatorState state in states.Values)
        {
            ReplicatorItem? item = state.Items.FirstOrDefault(i => i.TaskGuid == taskGuid);
            if (item != null)
                return item;
        }

        return null;
    }

    public static ReplicatorItem? FindReplicatorItem(this IReadOnlyDictionary<string, ReplicatorState> states, Guid taskGuid, out string? phase)
    {
        foreach (KeyValuePair<string, ReplicatorState> kvp in states)
        {
            ReplicatorItem? item = kvp.Value.Items.FirstOrDefault(i => i.TaskGuid == taskGuid);
            if (item != null)
            {
                phase = kvp.Key;
                return item;
            }
        }

        phase = null;
        return null;
    }

    /// <summary>
    /// Inserts a new item in the specified phase, immediately following
    /// the item whose TaskGuid is <paramref name="existingTaskGuid"/>.
    /// If the existing task is not found, the new item is simply appended.
    /// </summary>
    public static void InsertItemAfter(this IDictionary<string, ReplicatorState> states, string phaseName, Guid existingTaskGuid, ReplicatorItem newItem)
    {
        if (!states.TryGetValue(phaseName, out ReplicatorState? state))
        {
            throw new ArgumentException($"Phase '{phaseName}' not found in replicator states.", nameof(phaseName));
        }

        int idx = state.Items.FindIndex(i => i.TaskGuid == existingTaskGuid);
        if (idx >= 0)
        {
            state.Items.Insert(idx + 1, newItem);
        }
        else
        {
            // If we can't find the current item, just add it at the end
            state.Items.Add(newItem);
        }
    }
}