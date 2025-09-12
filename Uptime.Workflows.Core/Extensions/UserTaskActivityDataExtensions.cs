using System.Text.Json;

namespace Uptime.Workflows.Core.Extensions;

public static class UserTaskActivityDataExtensions
{
    public static T DeserializeTaskData<T>(this object data) where T : class
    {
        if (data is JsonElement jsonElement)
        {
            var result = jsonElement.Deserialize<T>();
            return result ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from JSON.");
        }

        if (data is T typedData)
        {
            return typedData;
        }

        throw new InvalidOperationException($"Unexpected data type: {data.GetType().Name}, expected {typeof(T).Name}");
    }
}