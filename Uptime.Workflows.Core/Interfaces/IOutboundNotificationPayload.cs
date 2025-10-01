using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IOutboundNotificationPayload
{
    WorkflowId WorkflowId { get; }
    string WorkflowType { get; }
    string SourceSiteUrl { get; }
    DateTimeOffset OccurredAtUtc { get; }
    Guid UniqueKey { get; }
}