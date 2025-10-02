using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

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
    IWorkflowOutboundNotifier? notifier,
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(workflowService,taskService, historyService, notifier, logger: logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;

    #region Public API

    /// <summary>
    /// Entry point for user-initiated task changes (approve/reject/delegate/…).
    /// Reconstructs the task activity, lets the workflow react, and persists state.
    /// </summary>
    public async Task<Result<Unit>> AlterTaskAsync(WorkflowEventType action, AlterTaskPayload payload, CancellationToken ct)
    {
        _logger.LogAlterTaskTriggered(WorkflowDefinition, WorkflowId, AssociationName);

        if (Machine.State.IsFinal())
        {
            _logger.LogAlreadyCompletedNoModification(WorkflowDefinition, WorkflowId);
            return Result<Unit>.Failure(ErrorCode.Conflict);
        }

        try
        {
            WorkflowActivityContext context = WorkflowActivityContextFactory.FromDatabase(
                taskGuid: payload.TaskGuid,
                phaseId: payload.PhaseId,
                assignedToSid: payload.AssignedToSid,
                dueDate: payload.DueDate,
                description: payload.Description,
                storageJson: payload.StorageJson);

            await OnTaskAlteredAsync(action, context, payload.ExecutorSid, payload.InputData, ct);
            await SaveWorkflowStateAsync(payload.ExecutorSid, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to alter workflow with ID {WorkflowId} task", WorkflowId.Value);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }
        
        return Result<Unit>.Success(new Unit());
    }

    #endregion

    #region Protected Hooks
 
    protected override Task OnWorkflowActivatedAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
    protected Task NotifyTasksCreatedAsync(string phaseName, bool isParallel, List<TaskProjection> tasks, CancellationToken ct)
    {
        if (Notifier is null) return Task.CompletedTask;

        var payload = new TasksCreatedPayload
        {
            OccurredAtUtc = DateTimeOffset.UtcNow,
            WorkflowId = WorkflowId,
            WorkflowType = GetType().Name,
            PhaseName = phaseName,
            IsParallelPhase = isParallel,
            Tasks = tasks,
            SourceSiteUrl = WorkflowContext.GetSiteUrl()
        };

        return Notifier.NotifyTasksCreatedAsync(payload, ct);
    }

    #endregion

    #region Abstract Members

    protected abstract Task OnTaskAlteredAsync(
        WorkflowEventType action, 
        WorkflowActivityContext activityContext, 
        PrincipalSid executorSid, Dictionary<string, string?> inputData, 
        CancellationToken ct);

    #endregion
}