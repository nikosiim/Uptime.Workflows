using Microsoft.Extensions.Logging;
using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Domain.Workflows;
using Uptime.Shared;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository,
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorActivityWorkflowBase<ApprovalWorkflowContext>(stateMachineFactory, repository, logger)
{
    private readonly IWorkflowRepository _repository = repository;

    private string? AssociationName => WorkflowContext.Storage.GetValueOrDefault(GlobalConstants.WorkflowStorageKeys.AssociationName);

    protected override IWorkflowDefinition WorkflowDefinition => new ApprovalWorkflowDefinition();

    protected override IReplicatorActivityProvider ActivityProvider  => new ApprovalWorkflowActivityProvider(_repository);
    
    protected override void ConfigureStateMachineAsync(CancellationToken cancellationToken)
    {
        Machine.Configure(WorkflowPhase.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowPhase.InProgress);

        Machine.Configure(WorkflowPhase.InProgress)
            .InitialTransition(ApprovalPhase.Approval) 
            .Permit(WorkflowTrigger.Cancel, WorkflowPhase.Cancelled)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowPhase.Completed);

        Machine.Configure(ApprovalPhase.Approval)
            .SubstateOf(WorkflowPhase.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ReplicatorPhases.Approval, cancellationToken))
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed, () => WorkflowContext.AnyTaskRejected)
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, ApprovalPhase.Signing, () => !WorkflowContext.AnyTaskRejected)
            .PermitDynamic(WorkflowTrigger.AllTasksCompleted, () => WorkflowContext.AnyTaskRejected ? WorkflowPhase.Completed : ApprovalPhase.Signing);

        Machine.Configure(ApprovalPhase.Signing)
            .SubstateOf(WorkflowPhase.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ReplicatorPhases.Signing, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed);

        Machine.Configure(WorkflowPhase.Completed)
            .OnEntry(() => Console.WriteLine("Workflow completed successfully."));
        Machine.Configure(WorkflowPhase.Cancelled)
            .OnEntry(() => Console.WriteLine("Workflow was cancelled."));
    }

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        base.OnWorkflowActivatedAsync(payload, cancellationToken);
        
        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        WorkflowContext.Outcome = ApprovalOutcome.Approved;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";
        
        return Task.CompletedTask;
    }
}