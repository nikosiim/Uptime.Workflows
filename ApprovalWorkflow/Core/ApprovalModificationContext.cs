namespace ApprovalWorkflow;

public record ApprovalModificationContext
{
    public required List<ApprovalTask> ApprovalTasks { get; init; }
}

public record ApprovalTask
{
    public required string AssignedToSid { get; init; }
    public required string TaskGuid { get; init; }
}