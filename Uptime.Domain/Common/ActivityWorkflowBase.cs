using Microsoft.Extensions.Logging;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public abstract class ActivityWorkflowBase<TContext>(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowStateRepository<TContext> stateRepository, 
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(stateMachineFactory, stateRepository, logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;
    
    public async Task AlterTaskCoreAsync(WorkflowTaskContext context)
    {
        if (CanAlterTask())
        {
            await AlterTaskInternalAsync(context);
            await SaveWorkflowStateAsync();
        }
    }
    
    protected abstract override void ConfigureStateMachine();
    protected abstract override void OnWorkflowActivated(IWorkflowPayload payload);
    protected abstract Task AlterTaskInternalAsync(WorkflowTaskContext context);

    private bool CanAlterTask()
    {
        if (Machine.CurrentState == WorkflowPhase.Completed)
        {
            _logger.LogDebug("Workflow is already completed. No modifications allowed.");
            return false;
        }

        return true;
    }
}