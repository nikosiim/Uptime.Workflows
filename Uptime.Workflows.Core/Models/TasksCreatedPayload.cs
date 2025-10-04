using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public sealed record TasksCreatedPayload : IOutboundNotificationPayload
{
    public string PayloadType => nameof(TasksCreatedPayload);
    public Guid UniqueKey { get; init; } = Guid.NewGuid();
    public required DateTimeOffset OccurredAtUtc { get; init; }
    public required string SourceSiteUrl { get; init; }
    public required WorkflowId WorkflowId { get; init; }
    public required string WorkflowType { get; init; }
    public string? PhaseId { get; init; }
    public IReadOnlyList<TaskProjection> Tasks { get; init; } = new List<TaskProjection>();
}

public sealed record TaskProjection(Guid TaskGuid, PrincipalSid AssignedToSid);