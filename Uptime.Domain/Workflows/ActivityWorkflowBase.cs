﻿using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class ActivityWorkflowBase<TContext>(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowStateRepository<TContext> stateRepository,
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(stateMachineFactory, stateRepository, logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;

    public async Task AlterTaskCoreAsync(WorkflowTaskContext context, CancellationToken cancellationToken)
    {
        if (CanAlterTask())
        {
            await AlterTaskInternalAsync(context, cancellationToken);
            await SaveWorkflowStateAsync(cancellationToken);
        }
    }

    protected abstract override void ConfigureStateMachineAsync(CancellationToken cancellationToken);
    protected abstract override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    protected abstract Task AlterTaskInternalAsync(WorkflowTaskContext context, CancellationToken cancellationToken);

    private bool CanAlterTask()
    {
        if (Machine.CurrentState.IsFinal())
        {
            _logger.LogDebug("Workflow is already completed. No modifications allowed.");
            return false;
        }

        return true;
    }
}