namespace Uptime.Domain.Enums;

public enum WorkflowHistoryEventType
{
    /// <summary>There is no specific event type for this workflow event.</summary>
    None,
    /// <summary>The workflow event concerns the workflow instance being initiated.</summary>
    WorkflowStarted,
    /// <summary>The workflow event concerns the workflow instance being completed.</summary>
    WorkflowCompleted,
    /// <summary>The workflow event concerns the workflow instance being cancelled.</summary>
    WorkflowCancelled,
    /// <summary>The workflow event concerns the workflow instance being deleted.</summary>
    WorkflowDeleted,
    /// <summary>The workflow event concerns a workflow task being created.</summary>
    TaskCreated,
    /// <summary>The workflow event concerns a workflow task being marked as complete.</summary>
    TaskCompleted,
    /// <summary>The workflow event concerns a workflow task being modified.</summary>
    TaskModified,
    /// <summary>The workflow event concerns changes to a workflow task being rolled back.</summary>
    TaskRolledBack,
    /// <summary>The workflow event concerns a workflow task being deleted.</summary>
    TaskDeleted,
    /// <summary>The workflow event concerns the workflow instance generating an error.</summary>
    WorkflowError,
    /// <summary>The workflow event concerns a comment being written for the workflow instance.</summary>
    WorkflowComment,
}