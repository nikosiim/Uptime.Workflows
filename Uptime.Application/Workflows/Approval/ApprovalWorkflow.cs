using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Domain.Workflows;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflow(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository, 
    IReplicatorPhaseBuilder<ApprovalTaskData> replicatorPhaseBuilder,
    IWorkflowActivityFactory<ApprovalTaskData> activityFactory, 
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorWorkflowBase<ApprovalWorkflowContext, ApprovalTaskData>(stateMachineFactory, repository, activityFactory, replicatorPhaseBuilder, logger)
{
    public static class Phases
    {
        public const string ApprovalPhase = "ApprovalPhase";
        public const string SigningPhase = "SigningPhase";
    }
    
    public static Dictionary<string, Func<IWorkflowPayload, WorkflowId, IEnumerable<ApprovalTaskData>>> PhaseConfiguration
        => new()
        {
            { Phases.ApprovalPhase, (payload, workflowId) => payload.GetApprovalTasks(workflowId) },
            { Phases.SigningPhase, (payload, workflowId) => payload.GetSigningTasks(workflowId) }
        };

    protected override void ConfigureStateMachineAsync(CancellationToken cancellationToken)
    {
        Machine.Configure(WorkflowPhase.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowPhase.Approval);
        
        Machine.Configure(WorkflowPhase.Approval)
            .OnEntryAsync(() => RunReplicatorAsync(Phases.ApprovalPhase, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Signing)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowPhase.Rejected);
        Machine.Configure(WorkflowPhase.Signing)
            .OnEntryAsync(() => RunReplicatorAsync(Phases.SigningPhase, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed);

        Machine.Configure(WorkflowPhase.Completed)
            .OnEntry(() => Console.WriteLine("Workflow completed successfully."));

        Machine.Configure(WorkflowPhase.Rejected)
            .OnEntry(() => Console.WriteLine("Workflow was rejected."));
    }

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        base.OnWorkflowActivatedAsync(payload, cancellationToken);
    }
}