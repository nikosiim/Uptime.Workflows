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
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(workflowService,taskService, historyService, logger: logger), IActivityWorkflowMachine
    where TContext : class, IWorkflowContext, new()
{
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;

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
            await SaveWorkflowStateAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to alter workflow with ID {WorkflowId} task", WorkflowId.Value);
            return Result<Unit>.Failure(ErrorCode.Unexpected);
        }
        
        return Result<Unit>.Success(new Unit());
    }
    
    protected abstract Task OnTaskAlteredAsync(WorkflowEventType action, WorkflowActivityContext activityContext, PrincipalSid executorSid,
        Dictionary<string, string?> inputData, CancellationToken ct);

    protected override Task OnWorkflowActivatedAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}