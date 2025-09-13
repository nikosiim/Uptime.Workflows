using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

public interface IWorkflowMachine
{
    Task<Result<Unit>> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Result<string> GetModificationContext(); 
    Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken cancellationToken);
    Task<Result<Unit>> CancelAsync(PrincipalId principalId, string? comment, CancellationToken cancellationToken);
    Result<Unit> Rehydrate(string storageJson, string phase, CancellationToken cancellationToken);
    Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken cancellationToken, bool autoCommit = true);
}