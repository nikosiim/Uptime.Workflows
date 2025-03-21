using Uptime.Domain.Common;
using Uptime.Domain.Entities;

namespace Uptime.Domain.Interfaces;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task<Result<Unit>> AlterTaskAsync(WorkflowTask workflowTask, Dictionary<string, string?> payload, CancellationToken cancellationToken);
}