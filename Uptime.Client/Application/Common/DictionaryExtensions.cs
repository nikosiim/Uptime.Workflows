using System.Globalization;
using System.Text.Json;

namespace Uptime.Client.Application.Common;

public static class DictionaryExtensions
{
    public const string ListSeparator = ";#";

    #region Get Values from JSON String
  
    /// <summary>
    /// Extracts a string value from a JSON string that represents a dictionary.
    /// </summary>
    public static string? GetValue(this string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
            return storage?.GetValueOrDefault(key);
        }
        catch (JsonException)
        {
            // Handle JSON parsing errors
        }

        return null;
    }

    #endregion

    /// <summary>
    /// Stores a DateTime value in UTC format (ISO 8601) or sets the key to null if the value is null.
    /// </summary>
    public static void SetValueAsDateTime(this Dictionary<string, string?> data, string key, DateTime? value)
    {
        data[key] = value?.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Stores a list of strings as a single string, using ";#" as a separator, or sets the key to null if the value is null.
    /// </summary>
    public static void SetValueAsList(this Dictionary<string, string?> data, string key, IEnumerable<string>? values)
    {
        if (values != null)
        {
            data[key] = string.Join(ListSeparator, values);
        }
        else
        {
            data[key] =  null;
        }
    }
}