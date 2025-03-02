using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowMachine
{
    BaseState CurrentState { get; }
    Task<BaseState> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Task CancelWorkflowAsync(string executor, string comment, CancellationToken cancellationToken);
    Task<bool> RehydrateAsync(Workflow instance, CancellationToken cancellationToken);
    Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken cancellationToken, bool autoCommit = true);
}