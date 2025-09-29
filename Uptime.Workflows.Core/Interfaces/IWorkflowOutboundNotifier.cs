using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces
{
    public interface IWorkflowOutboundNotifier
    {
        Task NotifyWorkflowStartedAsync(WorkflowStartedPayload payload, CancellationToken ct);
        Task NotifyTasksCreatedAsync(TasksCreatedPayload payload, CancellationToken ct);
    }
}