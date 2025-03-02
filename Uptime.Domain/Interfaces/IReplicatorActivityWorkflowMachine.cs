using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IReplicatorActivityWorkflowMachine : IWorkflowMachine
{
    Task<string> ModifyWorkflow(ModificationContext modificationContext, CancellationToken cancellationToken);
}