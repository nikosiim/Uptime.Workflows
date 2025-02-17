namespace Uptime.Domain.Enums;

public enum WorkflowPhase
{
    NotStarted = 0,
    InProgress = 1,
    Approval = 2,
    Signing = 3,
    Review = 4,
    Completed = 10,
    Cancelled = 15,
    Invalid = 99
}