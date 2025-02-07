﻿namespace Uptime.Application.Interfaces;

public interface IAlterTaskPayload
{
    int TaskId { get; }
    int WorkflowId { get; }
    Dictionary<string, string?> Storage { get; }
}