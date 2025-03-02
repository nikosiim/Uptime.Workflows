using Microsoft.Extensions.Logging;
using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Domain.Workflows;
using Uptime.Shared;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(
    IStateMachineFactory<BaseState, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository,
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorActivityWorkflowBase<ApprovalWorkflowContext>(stateMachineFactory, repository, logger)
{
    private readonly IWorkflowRepository _repository = repository;

    private string? AssociationName => WorkflowContext.Storage.GetValueOrDefault(GlobalConstants.WorkflowStorageKeys.AssociationName);

    protected override IWorkflowDefinition WorkflowDefinition => new ApprovalWorkflowDefinition();

    protected override IReplicatorActivityProvider ActivityProvider => new ApprovalWorkflowActivityProvider(_repository);

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
            .OnEntryAsync(() => RunReplicatorAsync(ReplicatorPhases.Approval, cancellationToken))
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed, () => WorkflowContext.AnyTaskRejected)
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, ApprovalPhase.Signing, () => !WorkflowContext.AnyTaskRejected)
            .PermitDynamic(WorkflowTrigger.AllTasksCompleted, () => WorkflowContext.AnyTaskRejected ? BaseState.Completed : ExtendedState.Signing);
        Machine.Configure(ExtendedState.Signing)
            .SubstateOf(BaseState.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ReplicatorPhases.Signing, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, BaseState.Completed);
    }

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        base.OnWorkflowActivatedAsync(payload, cancellationToken);

        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";
    }

    protected override Task<bool> OnWorkflowModifiedAsync(ModificationContext modificationContext, CancellationToken cancellationToken)
    {
        if (!WorkflowContext.ReplicatorStates.TryGetValue(modificationContext.PhaseId, out ReplicatorState? replicatorState))
        {
            return Task.FromResult(false);
        }

        ReplicatorItem? currentTaskItem = replicatorState.Items.FirstOrDefault(item => item.Status == ReplicatorItemStatus.InProgress);
        if (currentTaskItem == null)
        {
            return Task.FromResult(false);
        }

        var currentTaskData = currentTaskItem.Data.DeserializeTaskData<ApprovalTaskData>();

        replicatorState.Items.RemoveAll(item => item.Status == ReplicatorItemStatus.NotStarted);
        
        foreach (ContextTask newTask in modificationContext.ContextTasks ?? [])
        {
            ApprovalTaskData newTaskData = ApprovalTaskData.Copy(currentTaskData);
            newTaskData.AssignedTo = newTask.AssignedTo;

            replicatorState.Items.Add(new ReplicatorItem { Data = newTaskData, Status = ReplicatorItemStatus.NotStarted });
        }

        return Task.FromResult(true);
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        WorkflowContext.Outcome = ExtendedOutcome.Approved;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";

        return Task.CompletedTask;
    }
}