using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Domain.Workflows;
using Uptime.Shared;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository, 
    IReplicatorPhaseBuilder replicatorPhaseBuilder,
    IReplicatorActivityProvider activityFactory, 
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorActivityWorkflowBase<ApprovalWorkflowContext>(stateMachineFactory, repository, activityFactory, replicatorPhaseBuilder, logger)
{
    public static class ReplicatorPhases
    {
        public const string ApprovalPhase = "ApprovalPhase";
        public const string SigningPhase = "SigningPhase";
    }

    public static Dictionary<string, ReplicatorPhaseConfiguration> PhaseConfiguration => new()
    {
        {
            ReplicatorPhases.ApprovalPhase,
            new ReplicatorPhaseConfiguration
            {
                ActivityData = (payload, workflowId) => payload.GetApprovalTasks(workflowId),
                ReplicatorType = payload => payload.GetReplicatorType(ReplicatorPhases.ApprovalPhase)
            }
        },
        {
            ReplicatorPhases.SigningPhase,
            new ReplicatorPhaseConfiguration
            {
                ActivityData = (payload, workflowId) => payload.GetSigningTasks(workflowId),
                ReplicatorType = payload => payload.GetReplicatorType(ReplicatorPhases.ApprovalPhase)
            }
        }
    };

    protected string? AssociationName => WorkflowContext.Storage.GetValueOrDefault(GlobalConstants.WorkflowStorageKeys.AssociationName);
    
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
            .OnEntryAsync(() => RunReplicatorAsync(ReplicatorPhases.ApprovalPhase, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, ApprovalPhase.Signing);

        Machine.Configure(ApprovalPhase.Signing)
            .SubstateOf(WorkflowPhase.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ReplicatorPhases.SigningPhase, cancellationToken))
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