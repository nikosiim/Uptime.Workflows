using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Core;

public abstract class ActivityWorkflowBase<TContext>(
    IStateMachineFactory<BaseState, WorkflowTrigger> stateMachineFactory,
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService, 
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(stateMachineFactory, workflowService, taskService, historyService, logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;

    public async Task<Result<Unit>> AlterTaskAsync(WorkflowTask workflowTask, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        if (Machine.State.IsFinal())
        {
            _logger.LogInformation("Workflow is already completed. No modifications allowed.");
            return Result<Unit>.Failure("Workflow is already completed.");
        }

        try
        {
            var taskContext = new WorkflowTaskContext(workflowTask);

            await OnTaskAlteredAsync(taskContext, payload, cancellationToken);
            await SaveWorkflowStateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to alter workflow with ID {WorkflowId} task", WorkflowId.Value);
            return Result<Unit>.Failure("Task update failed");
        }
        
        return Result<Unit>.Success(new Unit());
    }
    
    protected virtual Task OnTaskAlteredAsync(WorkflowTaskContext context, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}