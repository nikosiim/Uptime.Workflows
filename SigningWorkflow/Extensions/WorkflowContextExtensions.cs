using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;

namespace SigningWorkflow;

internal static class WorkflowContextExtensions
{
    public static List<string> GetTaskSids(this IWorkflowContext context)
        => context.Storage.TryGetValueAsList(StorageKeys.TaskSids, out List<string> sids) ? sids : [];

    public static List<string> GetTaskPrincipalIds(this IWorkflowContext context)
        => context.Storage.TryGetValueAsList(StorageKeys.TaskPrincipalIds, out List<string> ids) ? ids : [];

    public static void SetTaskPrincipalIds(this IWorkflowContext context, IEnumerable<string> principalIds)
        => context.Storage.SetValue(StorageKeys.TaskPrincipalIds, string.Join(DictionaryExtensions.ListSeparator, principalIds));

    public static string? GetTaskDescription(this IWorkflowContext context)
        => context.Storage.GetValue(StorageKeys.TaskDescription);

    public static int? GetTaskDueDays(this IWorkflowContext context)
        => context.Storage.TryGetValue(StorageKeys.TaskDueDays, out string? days) && int.TryParse(days, out int result)
            ? result
            : null;

    private static class StorageKeys
    {
        public const string TaskDueDays      = "Workflow.Signing.Task.DueDays";
        public const string TaskDescription  = "Workflow.Signing.Task.Description";
        public const string TaskPrincipalIds = "Workflow.Signing.Task.PrincipalIds";
        public const string TaskSids         = "Workflow.Signing.Task.Sids";
    }
}