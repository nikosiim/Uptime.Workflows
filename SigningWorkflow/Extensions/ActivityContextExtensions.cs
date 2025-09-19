using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;

namespace SigningWorkflow;

public static class ActivityContextExtensions
{
    public static void SetTaskComment(this IWorkflowActivityContext context, string? comment)
        => context.Storage.SetValue(StorageKeys.TaskComment, comment);
    
    public static void SetTaskTitle(this IWorkflowActivityContext context, string? title)
        => context.Storage.SetValue(StorageKeys.TaskTitle, title);

    private static class StorageKeys
    {
        public const string TaskTitle   = "Activity.Signing.Task.Title";
        public const string TaskComment = "Activity.Signing.Task.Comment";
    }
}