using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task<Result<Unit>> AlterTaskAsync(AlterTaskPayload payload, CancellationToken cancellationToken);
}