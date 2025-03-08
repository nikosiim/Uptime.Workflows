namespace Uptime.Application.Workflows.Approval;

public record ApprovalModificationContext
{
    public required List<ApprovalTask> ApprovalTasks { get; init; }
}

public record ApprovalTask
{
    public required string AssignedTo { get; init; }
    public required string TaskGuid { get; init; }
}