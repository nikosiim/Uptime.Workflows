using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

public interface IWorkflowMachine
{
    Task<Result<Unit>> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Result<string> GetModificationContext(); 
    Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken cancellationToken);
    Task<Result<Unit>> CancelAsync(string executor, string comment, CancellationToken cancellationToken);
    Result<Unit> Rehydrate(Workflow instance, CancellationToken cancellationToken);
    Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken cancellationToken, bool autoCommit = true);
}