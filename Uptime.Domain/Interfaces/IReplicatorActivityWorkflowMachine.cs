using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IReplicatorActivityWorkflowMachine : IWorkflowMachine
{
    Task<string> ModifyWorkflowAsync(ModificationPayload modificationContext, CancellationToken cancellationToken);
}