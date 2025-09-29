using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Models;

public sealed record WorkflowStartedPayload(
    WorkflowId WorkflowId,
    string WorkflowType,
    PrincipalSid StartedBySid,
    IReadOnlyList<AssigneeProjection> Assignees,
    DateTimeOffset StartedAtUtc);

public sealed record AssigneeProjection(string PhaseName, PrincipalSid Sid);