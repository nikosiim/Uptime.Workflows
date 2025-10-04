using Workflows.Core.Interfaces;
using Workflows.Core.Common;

namespace Workflows.Core.Models;

public sealed class TaskUpdatedPayload : IOutboundNotificationPayload
{
    public string PayloadType => nameof(TaskUpdatedPayload);
    public Guid UniqueKey { get; init; } = Guid.NewGuid();
    public required DateTimeOffset OccurredAtUtc { get; init; }
    public required string SourceSiteUrl { get; init; }
    public required WorkflowId WorkflowId { get; init; }
    public required string WorkflowType { get; init; }
    public required Guid TaskGuid { get; init; }
    public required PrincipalSid AssignedToSid { get; init; }
    public required PrincipalSid ExecutorSid { get; init; }
    public required string? Outcome { get; init; }
    public required string? Status { get; init; }
}