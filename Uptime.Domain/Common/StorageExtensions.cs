using System.Text.Json;

namespace Uptime.Domain.Common;

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
}