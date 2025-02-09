﻿namespace Uptime.Domain.Enums;

public enum WorkflowPhase
{
    NotStarted = 0,
    Approval = 1,
    Signing = 2,
    Review = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6,
    Terminated = 7,
    Invalid = 99
}