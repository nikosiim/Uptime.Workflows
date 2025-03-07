using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class ActivityWorkflowBase<TContext>(
    IStateMachineFactory<BaseState, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository, 
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(stateMachineFactory, repository, logger: logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;
    
    public async Task<Result<Unit>> AlterTaskAsync(WorkflowTaskContext storedTaskContext, Dictionary<string, string?> alterTaskPayload, CancellationToken cancellationToken)
    {
        if (Machine.CurrentState.IsFinal())
        {
            _logger.LogDebug("Workflow is already completed. No modifications allowed.");
            return Result<Unit>.Failure("Workflow is already completed.");
        }

        try
        {
            await OnTaskChangedAsync(storedTaskContext, alterTaskPayload, cancellationToken);
            await SaveWorkflowStateAsync(cancellationToken);

            return Result<Unit>.Success(new Unit());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to alter workflow with ID {WorkflowId} task", WorkflowId.Value);
            return Result<Unit>.Failure("Task update failed");
        }
    }
    
    protected virtual Task OnTaskChangedAsync(WorkflowTaskContext context, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}