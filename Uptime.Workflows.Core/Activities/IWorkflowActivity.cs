namespace Uptime.Workflows.Core;

public interface IWorkflowActivity
{
    bool IsCompleted { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}