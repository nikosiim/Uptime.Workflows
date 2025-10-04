using Workflows.Core.Enums;
using Workflows.Core.Interfaces;

namespace Workflows.Core.Extensions;

public static class ActivityContextExtensionsCore
{
    public static string? GetTaskOutcome(this IWorkflowActivityContext context)
        => context.Storage.GetValue(StorageKeys.TaskOutcome);

    public static void SetTaskOutcome(this IWorkflowActivityContext context, string? outcome)
        => context.Storage.SetValue(StorageKeys.TaskOutcome, outcome);

    public static WorkflowTaskStatus GetTaskStatus(this IWorkflowActivityContext context)
        => context.Storage.TryGetValueAsEnum(StorageKeys.TaskStatus, out WorkflowTaskStatus status) ? status : WorkflowTaskStatus.NotStarted;

    public static void SetTaskStatus(this IWorkflowActivityContext context, WorkflowTaskStatus status)
        => context.Storage.SetValue(StorageKeys.TaskStatus, status.ToString());

    public static void SetTaskComment(this IWorkflowActivityContext context, string? comment)
        => context.Storage.SetValue(StorageKeys.TaskComment, comment);

    public static void SetTaskTitle(this IWorkflowActivityContext context, string? title)
        => context.Storage.SetValue(StorageKeys.TaskTitle, title);

    /// <summary>
    /// Activity context storage keys.
    /// Naming: Activity.[Workflow].[Phase].[Field]
    /// - [Workflow]: Workflow type
    /// - [Phase]: Activity phase or role
    /// - [Field]: Field name (Title, Comment, Status, etc)
    /// Use [Phase] to distinguish keys if activity repeats across phases.
    /// </summary>
    private static class StorageKeys
    {
        public const string TaskOutcome = "Activity.Task.Outcome";
        public const string TaskStatus  = "Activity.Task.Status";
        public const string TaskTitle   = "Activity.Task.Title";
        public const string TaskComment = "Activity.Task.Comment";
    }
}