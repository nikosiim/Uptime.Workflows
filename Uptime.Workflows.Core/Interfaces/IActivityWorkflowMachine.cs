using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Entities;

namespace Uptime.Workflows.Core.Interfaces;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task<Result<Unit>> AlterTaskAsync(WorkflowTask workflowTask, Dictionary<string, string?> payload, CancellationToken cancellationToken);
}