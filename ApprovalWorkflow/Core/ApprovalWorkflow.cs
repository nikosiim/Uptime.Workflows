using Microsoft.Extensions.Logging;
using System.Text.Json;
using Workflows.Core;
using Workflows.Core.Common;
using Workflows.Core.Enums;
using Workflows.Core.Extensions;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace ApprovalWorkflow;

public sealed class ApprovalWorkflow(
    IWorkflowService workflowService,
    ITaskService taskService,
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    IActivityActivator activator,
    IWorkflowOutboundNotifier notifier,
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorWorkflowBase<ApprovalWorkflowContext>(workflowService, taskService, historyService, principalResolver, notifier, logger)
{
    private readonly IPrincipalResolver _principalResolver = principalResolver;
    private readonly IHistoryService _historyService = historyService;

    protected override IWorkflowDefinition WorkflowDefinition => new ApprovalWorkflowDefinition();

    protected override void ConfigureStateMachineAsync(CancellationToken ct)
    {
        Machine.Configure(BaseState.NotStarted)
            .Permit(WorkflowTrigger.Start, BaseState.InProgress);

        Machine.Configure(BaseState.InProgress)
            .InitialTransition(ExtendedState.Approval)
            .Permit(WorkflowTrigger.Cancel, BaseState.Cancelled)
            .Permit(WorkflowTrigger.TaskRejected, BaseState.Completed);

        Machine.Configure(ExtendedState.Approval)
            .SubstateOf(BaseState.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ExtendedState.Approval.Value, ct))
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed, () => WorkflowContext.AnyTaskRejected)
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, ApprovalPhase.Signing, () => !WorkflowContext.AnyTaskRejected)
            .PermitDynamic(WorkflowTrigger.AllTasksCompleted, () => WorkflowContext.AnyTaskRejected ? BaseState.Completed : ExtendedState.Signing);

        Machine.Configure(ExtendedState.Signing)
            .SubstateOf(BaseState.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ExtendedState.Signing.Value, ct))
            .Permit(WorkflowTrigger.AllTasksCompleted, BaseState.Completed);
    }

    #region Notifications

    protected override bool ShouldNotifyOnTasksCreated(string phaseId)
    {
        return phaseId == ExtendedState.Approval.Value || phaseId == ExtendedState.Signing.Value;
    }

    protected override Task<IOutboundNotificationPayload?> BuildWorkflowStartedPayloadAsync(CancellationToken ct)
    {
        List<AssigneeProjection> assignees = WorkflowContext
            .ReplicatorStates
            .SelectMany(r => r.Value.Items)
            .Select(i => new AssigneeProjection(i.ActivityContext.PhaseId!, i.ActivityContext.AssignedToSid))
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

    protected override Task<IOutboundNotificationPayload?> BuildTasksCreatedPayloadAsync(string phaseId, List<TaskProjection> tasks, CancellationToken ct)
    {
        var payload = new TasksCreatedPayload
        {
            OccurredAtUtc = DateTimeOffset.UtcNow,
            WorkflowId = WorkflowId,
            WorkflowType = GetType().Name,
            PhaseId = phaseId,
            Tasks = tasks,
            SourceSiteUrl = WorkflowContext.GetSiteUrl()
        };

        return Task.FromResult<IOutboundNotificationPayload?>(payload);
    }

    protected override Task<IOutboundNotificationPayload?> BuildTaskUpdatedPayloadAsync(IWorkflowActivityContext ctx, PrincipalSid executorSid, CancellationToken ct)
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

    protected override Task<IOutboundNotificationPayload?> BuildWorkflowCompletedPayloadAsync(CancellationToken ct)
    {
        List<AssigneeProjection> assignees = WorkflowContext
            .ReplicatorStates
            .SelectMany(r => r.Value.Items)
            .Select(i => new AssigneeProjection(i.ActivityContext.PhaseId!, i.ActivityContext.AssignedToSid))
            .ToList();

        var payload = new WorkflowCompletedPayload
        {
            OccurredAtUtc = DateTimeOffset.UtcNow,
            WorkflowId = WorkflowId,
            WorkflowType = GetType().Name,
            Outcome = WorkflowContext.Outcome.Value,
            Assignees = assignees,
            SourceSiteUrl = WorkflowContext.GetSiteUrl()
        };

        return Task.FromResult<IOutboundNotificationPayload?>(payload);
    }

    #endregion

    protected override async Task OnWorkflowActivatedAsync(CancellationToken ct)
    {
        await base.OnWorkflowActivatedAsync(ct);

        string approverNames = await GetParticipantNamesAsync(ExtendedState.Approval.Value, ct);

        Principal initiator = await _principalResolver.ResolveBySidAsync(WorkflowContext.GetInitiatorSid(), ct);

        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud. Algataja: {initiator.Name}. Määratud: {approverNames}.";
    }

    protected override IReplicatorPhaseBuilder CreateReplicatorPhaseBuilder()
    {
        var phases = new Dictionary<string, ReplicatorPhaseConfiguration>
        {
            [ExtendedState.Approval.Value] = new()
            {
                ActivityData = ctx =>
                {
                    List<string> approverSids = ctx.GetTaskApproverSids();

                    return approverSids.Select(sid =>
                        WorkflowActivityContextFactory.CreateNew(
                            phaseId: ExtendedState.Approval.Value,
                            assignedToSid: (PrincipalSid)sid,
                            description: ctx.GetTaskApproverDescription(),
                            dueDate: ctx.GetTaskDueDate(TaskPhase.Approver)
                        )
                    );
                },
                ReplicatorType = ctx =>
                {
                    ReplicatorType? type = ctx.GetReplicatorType();
                    return type ?? ReplicatorType.Sequential;
                }
            },
            [ExtendedState.Signing.Value] = new()
            {
                ActivityData = ctx =>
                {
                    List<string> signerSids = ctx.GetTaskSignerSids();

                    return signerSids.Select(sid =>
                        WorkflowActivityContextFactory.CreateNew(
                            phaseId: ExtendedState.Signing.Value,
                            assignedToSid: (PrincipalSid)sid,
                            description: ctx.GetTaskSignerDescription(),
                            dueDate: ctx.GetTaskDueDate(TaskPhase.Signer)
                        )
                    );
                },
                ReplicatorType = _ => ReplicatorType.Sequential
            }
        };

        return new ReplicatorPhaseBuilder(phases);
    }

    protected override IUserTaskActivity CreateChildActivity(IWorkflowActivityContext ctx)
    {
        return ctx.PhaseId == ExtendedState.Signing.Value
            ? activator.Create<SigningTaskActivity>(WorkflowContext)
            : activator.Create<ApprovalTaskActivity>(WorkflowContext);
    }

    protected override void OnChildInitialized(string phaseId, IWorkflowActivityContext context, IWorkflowActivity activity)
    {
        if (phaseId == ExtendedState.Approval.Value)
        {
            // var taskData = data.DeserializeTaskData<ApprovalTaskData>();
        }
        else if (phaseId == ExtendedState.Signing.Value)
        {
            // var taskData = data.DeserializeTaskData<SigningTaskData>();
        }
    }

    protected override void OnChildCompleted(string phaseId, IUserTaskActivity activity)
    {
        if (phaseId == ExtendedState.Approval.Value && activity is ApprovalTaskActivity approval)
        {
            if (approval.TaskDelegatedToPrincipal != null)
            {
                string phase = ExtendedState.Approval.Value;
                Guid existing = approval.TaskGuid;

                WorkflowActivityContext newCtx = WorkflowActivityContextFactory.CreateNew(
                    phaseId: phase,
                    assignedToSid: approval.TaskDelegatedToPrincipal.Sid,
                    description: WorkflowContext.GetTaskApproverDescription(),
                    dueDate: WorkflowContext.GetTaskDueDate(TaskPhase.Approver));

                var item = new ReplicatorItem(newCtx);
                WorkflowContext.ReplicatorStates.InsertItemAfter(phase, existing, item);
            }
            else if (approval.IsTaskRejected)
            {
                WorkflowContext.AnyTaskRejected = true;
                WorkflowContext.ReplicatorStates.CancelAllItems();
            }
        }
        else if (phaseId == ExtendedState.Signing.Value && activity is SigningTaskActivity signing)
        {
            if (signing.IsTaskRejected)
            {
                WorkflowContext.AnyTaskRejected = true;
            }
        }
    }

    protected override string OnWorkflowModification()
    {
        base.OnWorkflowModification();

        ReplicatorState replicatorState = WorkflowContext.ReplicatorStates[Machine.State.Value];

        List<ReplicatorItem> activeItems = replicatorState.Items
            .Where(i => i.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress)
            .ToList();

        var context = new ApprovalModificationContext
        {
            ApprovalTasks = activeItems.Select(activeItem
                => new ApprovalTask
                {
                    AssignedToSid = activeItem.ActivityContext.AssignedToSid.Value,
                    TaskGuid = activeItem.TaskGuid.ToString()
                }).ToList()
        };

        return JsonSerializer.Serialize(context);
    }

    protected override async Task<bool> OnWorkflowModifiedAsync(ModificationPayload payload, CancellationToken ct)
    {
        if (!WorkflowContext.ReplicatorStates.TryGetValue(Machine.State.Value, out ReplicatorState? replicatorState))
            return false;

        ReplicatorItem? inProgressItem = replicatorState.Items.FirstOrDefault(item => item.Status == ReplicatorItemStatus.InProgress);
        if (inProgressItem == null || string.IsNullOrWhiteSpace(payload.ModificationContext))
            return false;

        var modificationContext = JsonSerializer.Deserialize<ApprovalModificationContext>(payload.ModificationContext);
        if (modificationContext == null)
            return true;

        WorkflowActivityContext activityContext = inProgressItem.ActivityContext;

        // ---- Diff old/new assignees (before modifying)
        HashSet<PrincipalSid> oldSids = replicatorState.Items
            .Where(item => item.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress)
            .Select(item => item.ActivityContext.AssignedToSid)
            .ToHashSet();

        replicatorState.Items.RemoveAll(item => item.Status == ReplicatorItemStatus.NotStarted);

        foreach (ApprovalTask task in modificationContext.ApprovalTasks)
        {
            var assignedTo = (PrincipalSid)task.AssignedToSid;
            if (assignedTo == activityContext.AssignedToSid) continue;

            WorkflowActivityContext newActivityContext =
                WorkflowActivityContextFactory.CreateNew(
                    phaseId: activityContext.PhaseId,
                    assignedToSid: assignedTo,
                    description: activityContext.Description,
                    dueDate: activityContext.DueDate
                );

            replicatorState.Items.Add(new ReplicatorItem(newActivityContext));
        }

        HashSet<PrincipalSid> newSids = replicatorState.Items
            .Where(item => item.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress)
            .Select(item => item.ActivityContext.AssignedToSid)
            .ToHashSet();

        await LogApprovalAssignmentChangesAsync(oldSids, newSids, payload.ExecutorSid, ct);

        return true;
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken ct)
    {
        WorkflowContext.Outcome = WorkflowContext.AnyTaskRejected ? ExtendedOutcome.Rejected : ExtendedOutcome.Approved;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";

        return Task.CompletedTask;
    }

    private async Task LogApprovalAssignmentChangesAsync(
        HashSet<PrincipalSid> oldSids,
        HashSet<PrincipalSid> newSids,
        PrincipalSid executorSid,
        CancellationToken ct)
    {
        List<PrincipalSid> added = newSids.Except(oldSids).ToList();
        List<PrincipalSid> removed = oldSids.Except(newSids).ToList();

        if (added.Count == 0 && removed.Count == 0)
            return;

        Principal executor = await _principalResolver.ResolveBySidAsync(executorSid, ct);

        HashSet<PrincipalSid> allChangedSids = added.Concat(removed).ToHashSet();
        await _principalResolver.EnsurePrincipalsCachedAsync(allChangedSids, ct);

        Principal?[] addedPrincipals = await Task.WhenAll(added.Select(sid => _principalResolver.TryResolveBySidAsync(sid, ct)));
        Principal?[] removedPrincipals = await Task.WhenAll(removed.Select(sid => _principalResolver.TryResolveBySidAsync(sid, ct)));

        List<string?> addedNames = addedPrincipals.Where(p => p != null).Select(p => p!.Name).ToList();
        List<string?> removedNames = removedPrincipals.Where(p => p != null).Select(p => p!.Name).ToList();

        var changes = new List<string>();
        if (addedNames.Count > 0)
            changes.Add($"Lisatud: {string.Join(", ", addedNames)}");
        if (removedNames.Count > 0)
            changes.Add($"Eemaldatud: {string.Join(", ", removedNames)}");

        var description = $"Kinnitaja muudatused: {string.Join(". ", changes)}.";

        await _historyService.CreateAsync(
            WorkflowId,
            WorkflowEventType.WorkflowComment,
            executor.Sid,
            description: description,
            ct: ct
        );
    }
}