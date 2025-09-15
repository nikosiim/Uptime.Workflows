using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;

namespace Uptime.Workflows.Core;

public static class WorkflowTaskContextExtensions
{
    #region Comment

    public static string? GetTaskComment(this IWorkflowTaskContext context)
        => context.Storage.GetValue(StorageKeys.TaskComment);

    public static void SetTaskComment(this IWorkflowTaskContext context, string? comment)
        => context.Storage.SetValue(StorageKeys.TaskComment, comment);

    #endregion

    #region Description

    public static string? GetTaskDescription(this IWorkflowTaskContext context)
        => context.Storage.GetValue(StorageKeys.TaskDescription);

    public static void SetTaskDescription(this IWorkflowTaskContext context, string? outcome)
        => context.Storage.SetValue(StorageKeys.TaskDescription, outcome);

    #endregion

    #region Id

    public static TaskId GetTaskId(this IWorkflowTaskContext context)
        => TaskId.Parse(context.Storage.GetValueOrDefault(StorageKeys.TaskId));

    public static void SetTaskId(this IWorkflowTaskContext context, TaskId id)
        => context.Storage[StorageKeys.TaskId] = id.Value.ToString();

    #endregion

    #region Outcome

    /// <summary>
    /// Gets the outcome/result string for this task (e.g. "Ootel", "Tagasilükatud", "Allkirjastatud").
    /// Returns null if not set.
    /// </summary>
    public static string? GetTaskOutcome(this IWorkflowTaskContext context)
        => context.Storage.GetValue(StorageKeys.TaskOutcome);

    public static void SetTaskOutcome(this IWorkflowTaskContext context, string? outcome)
        => context.Storage.SetValue(StorageKeys.TaskOutcome, outcome);

    #endregion

    #region Status

    public static WorkflowTaskStatus GetTaskStatus(this IWorkflowTaskContext context)
        => context.Storage.TryGetValueAsEnum(StorageKeys.TaskStatus, out WorkflowTaskStatus status) ? status : WorkflowTaskStatus.NotStarted;

    public static void SetTaskStatus(this IWorkflowTaskContext context, WorkflowTaskStatus status)
        => context.Storage.SetValue(StorageKeys.TaskStatus, status.ToString());

    #endregion

    #region Title

    public static string? GetTaskTitle(this IWorkflowTaskContext context)
        => context.Storage.GetValue(StorageKeys.TaskTitle);

    public static void SetTaskTitle(this IWorkflowTaskContext context, string? title)
        => context.Storage.SetValue(StorageKeys.TaskTitle, title);

    #endregion
    
    private static class StorageKeys
    {
        public const string TaskComment = "Task.Comment";
        public const string TaskDescription = "Task.Description";
        public const string TaskId = "Task.Id";
        public const string TaskOutcome = "Task.Outcome";
        public const string TaskStatus = "Task.Status";
        public const string TaskTitle = "Task.Title";
    }
}