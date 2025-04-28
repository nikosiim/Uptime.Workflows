using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Core;

public abstract class ActivityWorkflowBase<TContext>(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService,
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(workflowService,taskService, historyService, logger: logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;

    public async Task<Result<Unit>> AlterTaskAsync(WorkflowTask workflowTask, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        _logger.LogAlterTaskTriggered(WorkflowDefinition, WorkflowId, AssociationName);

        if (Machine.State.IsFinal())
        {
            _logger.LogAlreadyCompletedNoModification(WorkflowDefinition, WorkflowId);
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