namespace Workflows.Core.Enums;

public enum OutboundNotificationStatus
{
    Pending = 0,   // reserved for outbox usage (optional)
    Sent = 1,
    Failed = 2
}