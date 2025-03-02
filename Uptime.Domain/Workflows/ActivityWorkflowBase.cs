using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class ActivityWorkflowBase<TContext>(
    IStateMachineFactory<BaseState, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository, 
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(stateMachineFactory, repository, logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;
    
    public async Task AlterTaskAsync(WorkflowTaskContext storedTaskContext, Dictionary<string, string?> alterTaskPayload, CancellationToken cancellationToken)
    {
        if (CanAlterTask())
        {
            await OnTaskChangedAsync(storedTaskContext, alterTaskPayload, cancellationToken);
            await SaveWorkflowStateAsync(cancellationToken);
        }
    }
    
    protected virtual Task OnTaskChangedAsync(WorkflowTaskContext context, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

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