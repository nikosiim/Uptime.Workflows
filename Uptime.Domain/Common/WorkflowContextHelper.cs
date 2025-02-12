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
}