﻿namespace Uptime.Application.DTOs;

public record ModificationContext
{
    public required string WorkflowId { get; set; }
    public required string PhaseId { get; set; }
    public List<ContextTask>? ContextTasks { get; set; }
}

public record ContextTask
{
    public required string AssignedTo { get; set; }
    public required string TaskGuid { get; set; }
}