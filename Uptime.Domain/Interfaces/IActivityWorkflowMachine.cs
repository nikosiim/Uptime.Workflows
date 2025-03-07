using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task<Result<Unit>> AlterTaskAsync(WorkflowTaskContext context, Dictionary<string, string?> payload, CancellationToken cancellationToken);
}