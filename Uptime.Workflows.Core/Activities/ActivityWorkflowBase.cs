using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Core;

/// <summary>
/// Workflow base class with built-in support for user tasks.
/// 
/// Extends <see cref="WorkflowBase{TContext}"/> by adding:
/// • Ability to alter tasks mid-execution (via <c>AlterTaskAsync</c>).
/// • Hooks to react to task-level changes (<c>OnTaskAlteredAsync</c>).
/// 
/// New developers:
/// Use this base class when your workflow includes **activities that create
/// user tasks** (e.g. approvals, sign-offs). It provides the plumbing so your
/// workflow can handle user input while still persisting its state.
/// 
/// Think of this as “WorkflowBase + task awareness.”
/// </summary>
public abstract class ActivityWorkflowBase<TContext>(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(workflowService,taskService, historyService, principalResolver, logger: logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;

    public async Task<Result<Unit>> AlterTaskAsync(AlterTaskPayload payload, CancellationToken cancellationToken)
    {
        _logger.LogAlterTaskTriggered(WorkflowDefinition, WorkflowId, AssociationName);

        if (Machine.State.IsFinal())
        {
            _logger.LogAlreadyCompletedNoModification(WorkflowDefinition, WorkflowId);
            return Result<Unit>.Failure(ErrorCode.Conflict);
        }

        try
        {
            await OnTaskAlteredAsync(payload, cancellationToken);
            await SaveWorkflowStateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to alter workflow with ID {WorkflowId} task", WorkflowId.Value);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }
        
        return Result<Unit>.Success(new Unit());
    }
    
    protected virtual Task OnTaskAlteredAsync(AlterTaskPayload payload, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnWorkflowActivatedAsync(CancellationToken cancellationToken)
    {
        await PrepareInputDataAsync(cancellationToken);
    }

    /// <summary>
    /// Workflow must implement this to normalize user data (e.g., resolve SIDs to PrincipalIds).
    /// Called before any activities or replicator logic is executed.
    /// </summary>
    protected abstract Task PrepareInputDataAsync(CancellationToken cancellationToken);
}