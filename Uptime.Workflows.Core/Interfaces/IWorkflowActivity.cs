namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowActivity
{
    bool IsCompleted { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}