using System.Text.Json;

namespace Uptime.Application.Common;

internal static class StorageExtensions
{
    /// <summary>
    /// Deserializes the JSON storage into a dictionary.
    /// </summary>
    public static Dictionary<string, string?> DeserializeStorage(this string? storageJson)
    {
        return string.IsNullOrWhiteSpace(storageJson)
            ? new Dictionary<string, string?>()
            : JsonSerializer.Deserialize<Dictionary<string, string?>>(storageJson) ?? new Dictionary<string, string?>();
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