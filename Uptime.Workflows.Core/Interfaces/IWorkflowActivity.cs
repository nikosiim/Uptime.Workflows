namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowActivity
{
    bool IsCompleted { get; }
    Task ExecuteAsync(IWorkflowActivityContext context, CancellationToken ct);
}