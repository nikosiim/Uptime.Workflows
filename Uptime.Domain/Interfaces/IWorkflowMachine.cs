using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowMachine
{
    WorkflowPhase CurrentState { get; }
    Task<WorkflowPhase> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Task TriggerTransitionAsync(WorkflowTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default);
    Task<bool> RehydrateAsync(WorkflowId workflowId, CancellationToken cancellationToken);
}