using Uptime.Workflows.Core.Common;

namespace ApprovalWorkflow;

public record ApprovalModificationContext
{
    public required List<ApprovalTask> ApprovalTasks { get; init; }
}

public record ApprovalTask
{
    public required PrincipalSid AssignedToSid { get; init; }
    public required string TaskGuid { get; init; }
}