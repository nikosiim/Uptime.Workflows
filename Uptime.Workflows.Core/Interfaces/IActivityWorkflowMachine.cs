using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task<Result<Unit>> AlterTaskAsync(AlterTaskPayload payload, CancellationToken cancellationToken);
}