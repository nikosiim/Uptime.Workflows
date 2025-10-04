namespace Workflows.Api.Contracts;

// This enum is mapped to WorkflowTaskStatus in Workflows.Core.Enums
public enum WorkflowTaskStatus
{
    Invalid = 0,
    NotStarted = 1,
    Completed = 2,
    Cancelled = 3
}