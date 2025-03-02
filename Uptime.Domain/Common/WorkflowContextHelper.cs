using System.Text.Json;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

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

    public static object? Deserialize(Type contextType, string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Activator.CreateInstance(contextType);
        }

        try
        {
            return JsonSerializer.Deserialize(json, contextType);
        }
        catch (JsonException)
        {
            return Activator.CreateInstance(contextType);
        }
    }

    /// <summary>
    /// Serializes the workflow context to JSON.
    /// </summary>
    public static string Serialize<TContext>(TContext context) where TContext : IWorkflowContext, new()
    {
        return JsonSerializer.Serialize(context);
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