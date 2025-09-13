using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

public interface IWorkflowMachine
{
    Result<string> GetModificationContext(); 
    Task<Result<Unit>> StartAsync(StartWorkflowPayload payload, CancellationToken cancellationToken);
    Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken cancellationToken);
    Task<Result<Unit>> CancelAsync(CancelWorkflowPayload payload, CancellationToken cancellationToken);
    Result<Unit> Rehydrate(string storageJson, string phase, CancellationToken cancellationToken);
    Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken cancellationToken, bool autoCommit = true);
}