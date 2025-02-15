using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Domain.Workflows;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository, 
    IReplicatorPhaseBuilder replicatorPhaseBuilder,
    IReplicatorActivityProvider activityFactory, 
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorActivityWorkflowBase<ApprovalWorkflowContext>(stateMachineFactory, repository, activityFactory, replicatorPhaseBuilder, logger)
{
    private readonly ILogger<WorkflowBase<ApprovalWorkflowContext>> _logger1 = logger;

    public static class Phases
    {
        public const string ApprovalPhase = "ApprovalPhase";
        public const string SigningPhase = "SigningPhase";
    }

    public static Dictionary<string, ReplicatorPhaseConfiguration> PhaseConfiguration => new()
    {
        {
            Phases.ApprovalPhase,
            new ReplicatorPhaseConfiguration
            {
                ActivityData = (payload, workflowId) => payload.GetApprovalTasks(workflowId),
                ReplicatorType = payload => payload.GetReplicatorType(Phases.ApprovalPhase)
            }
        },
        {
            Phases.SigningPhase,
            new ReplicatorPhaseConfiguration
            {
                ActivityData = (payload, workflowId) => payload.GetSigningTasks(workflowId),
                ReplicatorType = payload => payload.GetReplicatorType(Phases.ApprovalPhase)
            }
        }
    };

    protected override void ConfigureStateMachineAsync(CancellationToken cancellationToken)
    {
        Machine.Configure(WorkflowPhase.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowPhase.Approval);
        
        Machine.Configure(WorkflowPhase.Approval)
            .OnEntryAsync(() => RunReplicatorAsync(Phases.ApprovalPhase, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Signing)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowPhase.Completed);
        Machine.Configure(WorkflowPhase.Signing)
            .OnEntryAsync(() => RunReplicatorAsync(Phases.SigningPhase, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed);

        Machine.Configure(WorkflowPhase.Completed);
        Machine.Configure(WorkflowPhase.Cancelled);
    }

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        base.OnWorkflowActivatedAsync(payload, cancellationToken);
    }

    protected override async Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        await base.OnWorkflowCompletedAsync(cancellationToken);

        string status = WorkflowContext.AnyTaskRejected ? "Rejected" : "Approved";

        _logger1.LogInformation("Approval Workflow completed with status: {status}", status);
    }
}