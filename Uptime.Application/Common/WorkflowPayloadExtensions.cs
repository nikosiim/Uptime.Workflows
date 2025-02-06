namespace Uptime.Application.Common;

internal static class WorkflowPayloadExtensions
{
    public static bool TryGetValueAs<T>(this Dictionary<string, object> data, string key, out T? result)
    {
        if (data.TryGetValue(key, out object? value) && value is T typedValue)
        {
            result = typedValue;
            return true;
        }

        result = default;
        return false;
    }

    public static T? TryGetValueAs<T>(this Dictionary<string, object> data, string key)
    {
        return data.TryGetValue(key, out object? value) && value is T typedValue ? typedValue : default;
    }

    public static bool TryGetValueAsList<T>(this Dictionary<string, object> data, string key, out List<T> result)
    {
        if (data.TryGetValue(key, out object? value) && value is List<T> listValue)
        {
            result = listValue;
            return true;
        }

        result = [];
        return false;
    }
}