namespace Uptime.Application.Interfaces;

public interface IWorkflowActivity
{
    bool IsCompleted { get; }
    Task ExecuteAsync();
}