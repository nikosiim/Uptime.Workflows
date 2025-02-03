using System.Text.Json;

namespace Uptime.Shared.Common;

public static class JsonHelper
{
    public static string? ExtractValue(string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var storage = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            return storage != null && storage.TryGetValue(key, out object? value) ? value?.ToString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}