namespace Uptime.Application.Interfaces;

public interface IActivityContext
{
    int TaskId { get; }
    int WorkflowId { get; }
}