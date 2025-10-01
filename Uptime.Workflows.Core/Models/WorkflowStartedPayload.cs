using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public sealed record WorkflowStartedPayload : IOutboundNotificationPayload
{
    public Guid UniqueKey { get; init; } = Guid.NewGuid();
    public required DateTimeOffset OccurredAtUtc { get; init; }
    public required string SourceSiteUrl { get; init; }
    public required WorkflowId WorkflowId { get; init; }
    public required string WorkflowType { get; init; }
    public required PrincipalSid StartedBySid { get; init; }
    public IReadOnlyList<AssigneeProjection> Assignees { get; init; } = new List<AssigneeProjection>();
};

public sealed record AssigneeProjection(string PhaseName, PrincipalSid Sid);