using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace SigningWorkflow;

public sealed class SigningWorkflow(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService,
    IActivityActivator activator,
    IWorkflowOutboundNotifier notifier,
    ILogger<WorkflowBase<BaseWorkflowContext>> logger)
    : ActivityWorkflowBase<BaseWorkflowContext>(workflowService, taskService, historyService, notifier, logger)
{
    private readonly ILogger<WorkflowBase<BaseWorkflowContext>> _logger = logger;

    private bool IsTaskRejected { get; set; }

    protected override IWorkflowDefinition WorkflowDefinition => new SigningWorkflowDefinition();
    
    protected override void ConfigureStateMachineAsync(CancellationToken ct)
    {
        Machine.Configure(BaseState.NotStarted)
            .Permit(WorkflowTrigger.Start, ExtendedState.Signing);
        Machine.Configure(ExtendedState.Signing)
            .OnEntryAsync(() => StartSigningTask(ct))
            .OnExit(OnSigningTaskCompleted)
            .Permit(WorkflowTrigger.TaskCompleted, BaseState.Completed)
            .Permit(WorkflowTrigger.TaskRejected, BaseState.Completed)
            .Permit(WorkflowTrigger.Cancel, BaseState.Cancelled);
        Machine.Configure(BaseState.Completed);
    }

    #region Notifications

    protected override Task<IOutboundNotificationPayload?> BuildWorkflowStartedPayloadAsync(CancellationToken ct)
    {
        List<string> signerSids = WorkflowContext.GetTaskSids();
        List<AssigneeProjection> assignees = signerSids
            .Select(s => new AssigneeProjection(ExtendedState.Signing.Value, (PrincipalSid)s))
            .ToList();

        var payload = new WorkflowStartedPayload
        {
            OccurredAtUtc = DateTimeOffset.UtcNow,
            WorkflowId = WorkflowId,
            WorkflowType = GetType().Name,
            StartedBySid = WorkflowContext.GetInitiatorSid(),
            Assignees = assignees,
            SourceSiteUrl = WorkflowContext.GetSiteUrl()
        };

        return Task.FromResult<IOutboundNotificationPayload?>(payload);
    }

    protected override Task<IOutboundNotificationPayload?> BuildWorkflowCompletedPayloadAsync(CancellationToken ct)
    {
        List<AssigneeProjection> assignees = WorkflowContext.GetTaskSids()
            .Select(sid => new AssigneeProjection(ExtendedState.Signing.Value, (PrincipalSid)sid))
            .ToList();
        
        var payload = new WorkflowCompletedPayload
        {
            OccurredAtUtc = DateTimeOffset.UtcNow,
            WorkflowId = WorkflowId,
            WorkflowType = GetType().Name,
            Outcome = WorkflowContext.Outcome.ToString(),
            Assignees = assignees,
            SourceSiteUrl = WorkflowContext.GetSiteUrl()
        };

        return Task.FromResult<IOutboundNotificationPayload?>(payload);
    }

    protected override Task<IOutboundNotificationPayload?> BuildTasksCreatedPayloadAsync(string phaseName, List<TaskProjection> tasks, CancellationToken ct)
    {
        var payload = new TasksCreatedPayload
        {
            OccurredAtUtc = DateTimeOffset.UtcNow,
            WorkflowId = WorkflowId,
            WorkflowType = GetType().Name,
            PhaseName = phaseName,
            Tasks = tasks,
            SourceSiteUrl = WorkflowContext.GetSiteUrl()
        };

        return Task.FromResult<IOutboundNotificationPayload?>(payload);
    }

    protected override Task<IOutboundNotificationPayload?> BuildTaskUpdatedPayloadAsync(
        IWorkflowActivityContext ctx, PrincipalSid executorSid, CancellationToken ct)
    {
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

        return Task.FromResult<IOutboundNotificationPayload?>(payload);
    }

    #endregion

    protected override async Task OnWorkflowActivatedAsync(CancellationToken ct)
    {
        await base.OnWorkflowActivatedAsync(ct);

        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";
    }

    private async Task StartSigningTask(CancellationToken ct)
    {
        List<string> signerSids = WorkflowContext.GetTaskSids();
        if (signerSids.Count < 1)
            throw new WorkflowValidationException(ErrorCode.Validation, "Signer SID not provided.");

        int? dueDays = WorkflowContext.GetTaskDueDays();
        DateTime? dueDate = dueDays.HasValue ? DateTime.UtcNow.AddDays(dueDays.Value) : null;

        WorkflowActivityContext activityContext = WorkflowActivityContextFactory.CreateNew(
            phaseId: null,
            assignedToSid: (PrincipalSid)signerSids.First(),
            WorkflowContext.GetTaskDescription(),
            dueDate);

        var taskActivity = activator.Create<SigningTaskActivity>(WorkflowContext);

        await taskActivity.ExecuteAsync(activityContext, ct);
        await OnSigningTaskCreatedAsync(activityContext, ct);
    }

    private async Task OnSigningTaskCreatedAsync(WorkflowActivityContext activityContext, CancellationToken ct)
    {
        var tasks = new List<TaskProjection>
        {
            new(activityContext.TaskGuid, ExtendedState.Signing.Value, activityContext.AssignedToSid)
        };

        await NotifyTasksCreatedAsync(ExtendedState.Signing.Value, tasks, ct);
        _logger.LogSigningTaskCreated(WorkflowDefinition, WorkflowId, AssociationName);
    }

    protected override async Task OnTaskAlteredAsync(WorkflowEventType action, IWorkflowActivityContext activityContext,
        PrincipalSid executorSid, Dictionary<string, string?> inputData, CancellationToken ct)
    {
        var activity = activator.Create<SigningTaskActivity>(WorkflowContext);
        await activity.ChangedTaskAsync(action, activityContext, executorSid, inputData, ct);

        IsTaskRejected = activity.IsTaskRejected;

        if (activity.IsCompleted)
        {
            WorkflowTrigger trigger = activity.IsTaskRejected
                ? WorkflowTrigger.TaskRejected
                : WorkflowTrigger.TaskCompleted;

            await TriggerTransitionAsync(trigger, ct);
        }
    }

    private void OnSigningTaskCompleted()
    {
        _logger.LogSigningTaskCompleted(WorkflowDefinition, WorkflowId, AssociationName);
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken ct)
    {
        WorkflowContext.Outcome = IsTaskRejected ? ExtendedOutcome.Rejected : ExtendedOutcome.Signed;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";

        return Task.CompletedTask;
    }
}