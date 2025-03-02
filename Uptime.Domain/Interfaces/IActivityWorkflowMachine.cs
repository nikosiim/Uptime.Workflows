using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task AlterTaskAsync(WorkflowTaskContext context, Dictionary<string, string?> payload, CancellationToken cancellationToken);
}