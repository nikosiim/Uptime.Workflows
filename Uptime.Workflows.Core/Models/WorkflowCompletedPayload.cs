using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Models;

public sealed record WorkflowCompletedPayload(WorkflowId WorkflowId, string WorkflowType, WorkflowOutcome Outcome, DateTimeOffset CompletedAtUtc)
{
    public IReadOnlyList<AssigneeProjection> Assignees { get; init; } = new List<AssigneeProjection>();
}