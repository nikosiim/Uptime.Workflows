using System.Text.Json;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Common;

public static class WorkflowContextHelper
{
    /// <summary>
    /// Deserializes the workflow context from JSON.
    /// </summary>
    public static TContext Deserialize<TContext>(string? json) where TContext : IWorkflowContext, new()
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new TContext();
        }
        try
        {
            return JsonSerializer.Deserialize<TContext>(json) ?? new TContext();
        }
        catch (JsonException)
        {
            return new TContext();
        }
    }
    
    /// <summary>
    /// Merges the storage of the updated context into the existing context.
    /// </summary>
    public static void MergeContext<TContext>(TContext existing, TContext updated) where TContext : IWorkflowContext, new()
    {
        // Assumes that IWorkflowContext has a Storage property of type Dictionary<string, string?>.
        existing.Storage.MergeWith(updated.Storage);
    }
    
    /// <summary>
    /// Merges new storage values with existing ones (overwrites only updated fields).
    /// </summary>
    public static void MergeWith(this Dictionary<string, string?> existingStorage, Dictionary<string, string?> newStorage)
    {
        foreach (KeyValuePair<string, string?> kvp in newStorage)
        {
            existingStorage[kvp.Key] = kvp.Value;
        }
    }
}