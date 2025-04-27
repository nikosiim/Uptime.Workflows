using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Core;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task<Result<Unit>> AlterTaskAsync(WorkflowTask workflowTask, Dictionary<string, string?> payload, CancellationToken cancellationToken);
}