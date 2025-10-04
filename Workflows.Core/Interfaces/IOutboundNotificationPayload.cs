using Workflows.Core.Common;

namespace Workflows.Core.Interfaces;

public interface IOutboundNotificationPayload
{
    Guid UniqueKey { get; }
    DateTimeOffset OccurredAtUtc { get; }
    string SourceSiteUrl { get; }
    WorkflowId WorkflowId { get; }
    string WorkflowType { get; }
    string PayloadType { get; }
}