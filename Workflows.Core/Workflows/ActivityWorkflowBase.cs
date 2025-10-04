using Microsoft.Extensions.Logging;
using Workflows.Core.Common;
using Workflows.Core.Enums;
using Workflows.Core.Extensions;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace Workflows.Core;

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
    public async Task<Result<Unit>> AlterTaskAsync(WorkflowEventType action, UpdateTaskPayload payload, CancellationToken ct)
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
            await NotifyTaskUpdatedAsync(context, payload.ExecutorSid, ct);
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

    protected virtual Task<IOutboundNotificationPayload?> BuildTasksCreatedPayloadAsync(string phaseId, List<TaskProjection> tasks, CancellationToken ct)
    {
        /*
        var payload = new TasksCreatedPayload
        {
            OccurredAtUtc = DateTimeOffset.UtcNow,
            WorkflowId = WorkflowId,
            WorkflowType = GetType().Name,
            PhaseId = phaseId,
            Tasks = tasks,
            SourceSiteUrl = WorkflowContext.GetSiteUrl()
        };
        */

        return Task.FromResult<IOutboundNotificationPayload?>(null);
    }
    protected virtual Task<IOutboundNotificationPayload?> BuildTaskUpdatedPayloadAsync(IWorkflowActivityContext ctx, PrincipalSid executorSid, CancellationToken ct)
    {
        /*
            var payload = new TaskUpdatedPayload
            {
                OccurredAtUtc = DateTimeOffset.UtcNow,
                SourceSiteUrl = WorkflowContext.GetSiteUrl(),
                WorkflowId = WorkflowId,
                WorkflowType = GetType().Name,
                TaskGuid = ctx.TaskGuid,
                AssignedToSid = ctx.AssignedToSid,
                ExecutorSid = executorSid,
                Outcome = ctx.GetTaskOutcome(),
                Status = ctx.GetTaskStatus().ToString()
            };

            If you want per-phase filtering for task updates as well, you'd need:
            if (ctx.PhaseId is not "Phase1" and not "Phase4")
                return Task.FromResult<IOutboundNotificationPayload?>(null);
        */

        return Task.FromResult<IOutboundNotificationPayload?>(null);
    }

    #endregion

    #region Protected Internals 

    protected override Task OnWorkflowActivatedAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
    protected async Task NotifyTasksCreatedAsync(string phase, List<TaskProjection> tasks, CancellationToken ct)
    {
        IOutboundNotificationPayload? payload = await BuildTasksCreatedPayloadAsync(phase, tasks, ct);
        if (payload is null || Notifier is null) return;

        await Notifier.NotifyAsync(WorkflowEvents.WorkflowTasksCreated, payload, ct);
    }
    protected async Task NotifyTaskUpdatedAsync(IWorkflowActivityContext ctx, PrincipalSid executorSid, CancellationToken ct)
    {
        IOutboundNotificationPayload? payload = await BuildTaskUpdatedPayloadAsync(ctx, executorSid, ct);
        if (payload is null || Notifier is null) return;

        await Notifier.NotifyAsync(WorkflowEvents.WorkflowTaskUpdated, payload, ct);
    }

    #endregion

    #region Abstract Members

    protected abstract Task OnTaskAlteredAsync(
        WorkflowEventType action,
        IWorkflowActivityContext activityContext, 
        PrincipalSid executorSid, 
        Dictionary<string, string?> inputData, 
        CancellationToken ct);

    #endregion
}