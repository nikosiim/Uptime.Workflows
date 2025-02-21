namespace Uptime.Domain.Common; 

public static class ReplicatorDictionaryExtensions
{
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
}