using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public sealed record WorkflowCompletedPayload : IOutboundNotificationPayload
{
    public string PayloadType => nameof(WorkflowCompletedPayload);
    public Guid UniqueKey { get; init; } = Guid.NewGuid();
    public required DateTimeOffset OccurredAtUtc { get; init; }
    public required string SourceSiteUrl { get; init; }
    public required WorkflowId WorkflowId { get; init; }
    public required string WorkflowType { get; init; }
    public required string? Outcome { get; init; }
    public IReadOnlyList<AssigneeProjection> Assignees { get; init; } = new List<AssigneeProjection>();
}