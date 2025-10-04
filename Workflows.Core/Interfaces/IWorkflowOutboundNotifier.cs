namespace Workflows.Core.Interfaces;

public interface IWorkflowOutboundNotifier
{
    Task NotifyAsync(string eventName, IOutboundNotificationPayload payload, CancellationToken ct = default);
}