using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IReplicatorActivityWorkflowMachine : IWorkflowMachine
{
    ModificationContext? GetModificationContext(string phaseId);
    Task<string> ModifyWorkflowAsync(ModificationContext modificationContext, CancellationToken cancellationToken);
}