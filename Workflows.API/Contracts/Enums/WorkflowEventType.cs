namespace Workflows.Api.Contracts;

public enum WorkflowEventType
{
    None = 0,
    WorkflowStarted = 1,
    WorkflowCompleted = 2,
    WorkflowCancelled = 3,
    WorkflowDeleted = 4,
    WorkflowError = 5,
    WorkflowComment = 6,
    TaskCreated = 7,
    TaskCompleted = 8,
    TaskModified = 9,
    TaskRolledBack = 10,
    TaskDeleted = 11,
    TaskRejected = 12,
    TaskCancelled = 13,
    TaskDelegated = 14
}