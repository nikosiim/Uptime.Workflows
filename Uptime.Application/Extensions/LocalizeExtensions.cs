using Uptime.Application.Resources;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Extensions;

public static class LocalizeExtensions
{
    public static string GetTranslation(this WorkflowTaskStatus status)
    {
        var resourceKey = status.ToString();
        string? localized = TaskStatusResource.ResourceManager.GetString(resourceKey);

        return localized ?? resourceKey;
    }

    public static string? GetTranslation(this WorkflowOutcome outcome)
    {
        var resourceKey = outcome.ToString();
        if (resourceKey != null)
        {
            string? localized = WorkflowOutcomeResource.ResourceManager.GetString(resourceKey);
            
            return localized ?? resourceKey;
        }
        
        return resourceKey;
    }
}