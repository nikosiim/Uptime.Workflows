using System.Text.Json;

namespace Uptime.Application.Common;

internal static class StorageExtensions
{
    public static T DeserializeTaskData<T>(this object data) where T : class
    {
        if (data is JsonElement jsonElement)
        {
            var result = jsonElement.Deserialize<T>();
            if (result is null)
                throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from JSON.");

            return result;
        }

        if (data is T typedData)
        {
            return typedData;
        }

        throw new InvalidOperationException($"Unexpected data type: {data.GetType().Name}, expected {typeof(T).Name}");
    }
}