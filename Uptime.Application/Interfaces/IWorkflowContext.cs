namespace Uptime.Application.Interfaces;

public interface IWorkflowContext
{
    Dictionary<string, object?> Storage { get; protected set; }
}