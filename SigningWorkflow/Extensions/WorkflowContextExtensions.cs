using Workflows.Core.Extensions;
using Workflows.Core.Interfaces;

namespace SigningWorkflow;

internal static class WorkflowContextExtensions
{
    public static List<string> GetTaskSids(this IWorkflowContext context)
        => context.Storage.TryGetValueAsList(StorageKeys.TaskSids, out List<string> sids) ? sids : [];
    
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
        public const string TaskSids         = "Workflow.Signing.Task.Sids";
    }
}