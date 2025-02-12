namespace Uptime.Domain.Interfaces;

public interface IWorkflowActivity
{
    bool IsCompleted { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}