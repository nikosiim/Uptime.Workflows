﻿
namespace Uptime.Shared.Models.Workflows;

public record ModificationContextResponse
{
    public string? Executor { get; set; }
    public int WorkflowId { get; set; }
    public required string PhaseId { get; set; }
    public List<ContextTaskResponse>? ContextTasks { get; set; }
}

public record ContextTaskResponse
{
    public required string AssignedTo { get; set; }
    public required string TaskGuid { get; set; }
}