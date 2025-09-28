using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace ApprovalWorkflow;

public sealed class ApprovalWorkflow(
    IWorkflowService workflowService,
    ITaskService taskService,
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorWorkflowBase<ApprovalWorkflowContext>(workflowService, taskService, historyService, logger)
{
    private readonly ITaskService _taskService = taskService;
    private readonly IHistoryService _historyService = historyService;

    protected override IWorkflowDefinition WorkflowDefinition => new ApprovalWorkflowDefinition();
    
    protected override IReplicatorActivityProvider ActivityProvider 
        => new ApprovalWorkflowActivityProvider(_taskService, _historyService, principalResolver, WorkflowContext);

    protected override void ConfigureStateMachineAsync(CancellationToken cancellationToken)
    {
        Machine.Configure(BaseState.NotStarted)
            .Permit(WorkflowTrigger.Start, BaseState.InProgress);
        Machine.Configure(BaseState.InProgress)
            .InitialTransition(ExtendedState.Approval)
            .Permit(WorkflowTrigger.Cancel, BaseState.Cancelled)
            .Permit(WorkflowTrigger.TaskRejected, BaseState.Completed);
        Machine.Configure(ExtendedState.Approval)
            .SubstateOf(BaseState.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ExtendedState.Approval.Value, cancellationToken))
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed, () => WorkflowContext.AnyTaskRejected)
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, ApprovalPhase.Signing, () => !WorkflowContext.AnyTaskRejected)
            .PermitDynamic(WorkflowTrigger.AllTasksCompleted, () => WorkflowContext.AnyTaskRejected ? BaseState.Completed : ExtendedState.Signing);
        Machine.Configure(ExtendedState.Signing)
            .SubstateOf(BaseState.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ExtendedState.Signing.Value, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, BaseState.Completed);
    }
    
    protected override async Task OnWorkflowActivatedAsync(CancellationToken cancellationToken)
    {
        await base.OnWorkflowActivatedAsync(cancellationToken);
        
        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";
    }

    protected override Task PrepareInputDataAsync(CancellationToken cancellationToken)
    {
        // Resolve SIDs to Principal for parallel tasks
        // TODO: Think through what would be best place to decide whether there are parallel tasks or not
        return Task.CompletedTask;
    }

    protected override IReplicatorPhaseBuilder CreateReplicatorPhaseBuilder()
    {
        ExtendedState x = ExtendedState.Approval;

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

    protected override Task<bool> OnWorkflowModifiedAsync(ModificationPayload payload, CancellationToken cancellationToken)
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

    protected override Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        WorkflowContext.Outcome = WorkflowContext.AnyTaskRejected ? ExtendedOutcome.Rejected : ExtendedOutcome.Approved;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";

        return Task.CompletedTask;
    }
}