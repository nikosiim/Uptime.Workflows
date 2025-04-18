using Uptime.Client.Resources;

namespace Uptime.Client.Application.Common;

public static class WorkflowTaskResources
{
    public static string? Get(string? resourceKey)
    {
        if (!string.IsNullOrWhiteSpace(resourceKey))
        {
            string? localized = WorkflowTaskResource.ResourceManager.GetString(resourceKey);
            return localized ?? resourceKey;
        }
        return resourceKey;
    }
}

public static class WorkflowResources
{
    public static string? Get(string? resourceKey)
    {
        if (!string.IsNullOrWhiteSpace(resourceKey))
        {
            string? localized = WorkflowResource.ResourceManager.GetString(resourceKey);
            return localized ?? resourceKey;
        }
        return resourceKey;
    }
}

public static class LocalizeExtensions
{
    public static string GetTranslation(this WorkflowEventType status)
    {
        var resourceKey = status.ToString();
        string? localized = WorkflowHistoryResource.ResourceManager.GetString(resourceKey);

        return localized ?? resourceKey;
    }
}