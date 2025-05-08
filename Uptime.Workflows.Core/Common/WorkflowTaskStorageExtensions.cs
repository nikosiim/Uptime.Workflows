using System.Text.Json;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Common;

public static class WorkflowTaskStorageExtensions
{

    public static IReadOnlyList<AssigneeRef> GetAssignments(this WorkflowTask task)
    {
        if (string.IsNullOrWhiteSpace(task.StorageJson))
            return [];

        using JsonDocument doc = JsonDocument.Parse(task.StorageJson);

        if (!doc.RootElement.TryGetProperty(WorkflowStorageKeys.Assignments, out JsonElement arr) || arr.ValueKind != JsonValueKind.Array)
            return [];

        var list = new List<AssigneeRef>(arr.GetArrayLength());
        foreach (JsonElement el in arr.EnumerateArray())
        {
            list.Add(new AssigneeRef
            {
                Kind        = Enum.Parse<PrincipalKind>(el.GetProperty("Type").GetString()!, true),
                IdOrName          = el.GetProperty("Id").GetString()!,
                DisplayName = el.TryGetProperty("DisplayName", out JsonElement dn) ? dn.GetString() : null
            });
        }
        return list;
    }
}
