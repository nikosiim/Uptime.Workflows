namespace Uptime.Shared.Enums;

public enum WorkflowStatus
{
    NotStarted = 1,
    InProgress = 2,
    Rejected = 3,
    Completed = 4,
    Cancelled = 5,
    Terminated = 6,
    Invalid = 7,
    ApprovalInProgress = 8,
    SigningInProgress = 9
}