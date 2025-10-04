namespace Workflows.Core.Common;

public static class WorkflowEvents
{
    public const string WorkflowStarted = "started";
    public const string WorkflowTasksCreated = "tasks-created";
    public const string WorkflowTaskUpdated = "task-updated";
    public const string WorkflowCompleted = "completed";
    public const string WorkflowCancelled = "cancelled";
}