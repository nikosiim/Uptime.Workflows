using System.Globalization;
using System.Text.Json;

namespace Uptime.Shared.Extensions;

public static class DictionaryExtensions
{
    public const string ListSeparator = ";#";

    #region Get Values

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
    /// Gets an enum value from the dictionary, assuming it is stored as a string.
    /// Returns the default enum value if the key is missing or invalid.
    /// </summary>
    public static TEnum GetValueAsEnum<TEnum>(this Dictionary<string, string?> data, string key) where TEnum : struct
    {
        if (data.TryGetValue(key, out string? value) && Enum.TryParse(value, out TEnum parsedEnum))
        {
            return parsedEnum;
        }

        return default; // Returns the first value of the enum
    }

    /// <summary>
    /// Gets a list of strings from the dictionary, assuming values are separated by ";#".
    /// Returns an empty list if the key is missing or empty.
    /// </summary>
    public static List<string> GetValueAsList(this Dictionary<string, string?> data, string key)
    {
        if (data.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value))
        {
            return value.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        return [];
    }

    #endregion

    #region Get Values from JSON String

    /// <summary>
    /// Extracts a DateTime value from a JSON string.
    /// </summary>
    public static DateTime GetValueAsDateTime(this string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json))
            return DateTime.MinValue;

        try
        {
            var storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
            return storage?.GetValueAsDateTime(key) ?? DateTime.MinValue;
        }
        catch (JsonException)
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// Extracts an enum value from a JSON string.
    /// </summary>
    public static TEnum GetValueAsEnum<TEnum>(this string? json, string key) where TEnum : struct
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            var storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
            return storage?.GetValueAsEnum<TEnum>(key) ?? default;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Extracts a list of strings from a JSON string.
    /// </summary>
    public static List<string> GetValueAsList(this string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            var storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
            return storage?.GetValueAsList(key) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
  
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
            return storage?.GetValue(key);
        }
        catch (JsonException)
        {
            // Handle JSON parsing errors
        }

        return null;
    }

    #endregion

    #region Try Get Values

    /// <summary>
    /// Tries to get a value as a DateTime (assumes UTC format string).
    /// </summary>
    public static bool TryGetValueAsDateTime(this Dictionary<string, string?> data, string key, out DateTime? result)
    {
        if (data.TryGetValue(key, out string? value) && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
        {
            result = dateTime;
            return true;
        }

        result = null;
        return false;
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
    /// Generic method to retrieve any value type, ensuring it is correctly parsed.
    /// </summary>
    public static bool TryGetValueAs<T>(this Dictionary<string, string?> data, string key, out T? result)
    {
        if (data.TryGetValue(key, out string? value) && value is not null)
        {
            try
            {
                result = (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                // Ignore conversion errors
            }
        }

        result = default;
        return false;
    }

    #endregion

    #region Set Values

    /// <summary>
    /// Stores a DateTime value in UTC format (ISO 8601) or sets the key to null if the value is null.
    /// </summary>
    public static void SetValueAsDateTime(this Dictionary<string, string?> data, string key, DateTime? value)
    {
        data[key] = value?.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Stores an enum value as a string or sets the key to null if the value is null.
    /// </summary>
    public static void SetValueAsEnum<TEnum>(this Dictionary<string, string?> data, string key, TEnum? value) where TEnum : struct
    {
        data[key] = value?.ToString();
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

    /// <summary>
    /// Stores a string value in the dictionary or sets the key to null if the value is null.
    /// </summary>
    public static void SetValue(this Dictionary<string, string?> data, string key, string? value)
    {
        data[key] = value;
    }

    #endregion
}