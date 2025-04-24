using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Entities;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowMachine
{
    Task<Result<Unit>> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Result<string> GetModificationContext(); 
    Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken cancellationToken);
    Task<Result<Unit>> CancelAsync(string executor, string comment, CancellationToken cancellationToken);
    Result<Unit> RehydrateAsync(Workflow instance, CancellationToken cancellationToken);
    Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken cancellationToken, bool autoCommit = true);
}