﻿using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowMachine
{
    WorkflowPhase CurrentState { get; }
    Task<WorkflowPhase> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    Task CancelWorkflowAsync(CancellationToken cancellationToken);
    Task<bool> RehydrateAsync(WorkflowId workflowId, CancellationToken cancellationToken);
    Task TriggerTransitionAsync(WorkflowTrigger trigger, CancellationToken cancellationToken, bool autoCommit = true);
}