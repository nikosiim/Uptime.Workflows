using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

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

    protected override async Task OnWorkflowActivatedAsync(CancellationToken ct)
    {
        await base.OnWorkflowActivatedAsync(ct);

        Principal initiator = await _principalResolver.ResolveBySidAsync(WorkflowContext.GetInitiatorSid(), ct);

        // Collect all SIDs from the built replicator states
        if (WorkflowContext.ReplicatorStates.TryGetValue(ExtendedState.Approval.Value, out ReplicatorState? approval))
        {
            List<Principal> approvers = [];
            foreach (ReplicatorItem item in approval.Items)
            {
                Principal p = await _principalResolver.ResolveBySidAsync(item.ActivityContext.AssignedToSid, ct);
                approvers.Add(p);
            }

            string approverNames = string.Join(", ", approvers.Select(p => p.Name));
            WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud. Algataja: {initiator.Name}. Määratud: {approverNames}.";
        }
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
                    AssignedToSid = activeItem.ActivityContext.AssignedToSid,
                    TaskGuid = activeItem.TaskGuid.ToString()
                }).ToList()
        };

        return JsonSerializer.Serialize(context);
    }

    protected override Task<bool> OnWorkflowModifiedAsync(ModificationPayload payload, CancellationToken ct)
    {
        if (!WorkflowContext.ReplicatorStates.TryGetValue(Machine.State.Value, out ReplicatorState? replicatorState))
            return Task.FromResult(false);

        ReplicatorItem? inProgressItem = replicatorState.Items.FirstOrDefault(item => item.Status == ReplicatorItemStatus.InProgress);
        if (inProgressItem == null || string.IsNullOrWhiteSpace(payload.ModificationContext))
            return Task.FromResult(false);

        var modificationContext = JsonSerializer.Deserialize<ApprovalModificationContext>(payload.ModificationContext);
        if (modificationContext == null)
            return Task.FromResult(true);

        WorkflowActivityContext activityContext = inProgressItem.ActivityContext;

        replicatorState.Items.RemoveAll(item => item.Status == ReplicatorItemStatus.NotStarted);

        foreach (ApprovalTask task in modificationContext.ApprovalTasks)
        {
            if (task.AssignedToSid == activityContext.AssignedToSid) continue;

            WorkflowActivityContext newActivityContext =
                WorkflowActivityContextFactory.CreateNew(
                    phaseId: activityContext.PhaseId,
                    assignedToSid: task.AssignedToSid,
                    description: activityContext.Description,
                    dueDate: activityContext.DueDate
                );

            replicatorState.Items.Add(new ReplicatorItem(newActivityContext));
        }

        return Task.FromResult(true);
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken ct)
    {
        WorkflowContext.Outcome = WorkflowContext.AnyTaskRejected ? ExtendedOutcome.Rejected : ExtendedOutcome.Approved;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";

        return Task.CompletedTask;
    }
}