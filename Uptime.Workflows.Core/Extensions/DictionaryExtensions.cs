using System.Globalization;

namespace Uptime.Workflows.Core.Extensions;

public static class DictionaryExtensions
{
    public const string ListSeparator = ";#";
    
    /// <summary>
    /// Retrieves a string value from a dictionary, if the key exists.
    /// </summary>
    public static string? GetValue(this Dictionary<string, string?> data, string key)
    {
        return data.GetValueOrDefault(key);
    }

    /// <summary>
    /// Gets a DateTime value from the dictionary, assuming it is stored as a UTC string.
    /// Returns DateTime.MinValue if the key is missing or invalid.
    /// </summary>
    public static DateTime GetValueAsDateTime(this Dictionary<string, string?> data, string key)
    {
        if (data.TryGetValue(key, out string? value) &&
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
        {
            return dateTime;
        }

        return DateTime.MinValue;
    }

    /// <summary>
    /// Tries to get a value as an Enum.
    /// </summary>
    public static bool TryGetValueAsEnum<TEnum>(this Dictionary<string, string?> data, string key, out TEnum result) where TEnum : struct
    {
        if (data.TryGetValue(key, out string? value) && Enum.TryParse(value, out TEnum parsedEnum))
        {
            result = parsedEnum;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Tries to get a value as a List of strings (expects values to be separated by ";#").
    /// </summary>
    public static bool TryGetValueAsList(this Dictionary<string, string?> data, string key, out List<string> result)
    {
        if (data.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value))
        {
            result = value.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
            return true;
        }

        result = [];
        return false;
    }

    /// <summary>
    /// Stores a string value in the dictionary or sets the key to null if the value is null.
    /// </summary>
    public static void SetValue(this Dictionary<string, string?> data, string key, string? value)
    {
        data[key] = value;
    }
}