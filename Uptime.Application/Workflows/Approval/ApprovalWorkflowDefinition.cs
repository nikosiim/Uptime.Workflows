using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalWorkflowDefinition : IWorkflowDefinition
{
    public string WorkflowBaseId => "ApprovalWorkflow";

    public WorkflowConfiguration Configuration { get; } = new()
    {
        Phases =
        [
            new PhaseConfiguration
            {
                Id = ReplicatorPhases.Approval,
                DisplayName = "Approval",
                SupportsSequential = true,
                SupportsParallel = true,
                Actions = ["Approval", "Rejection", "Forward", "Cancellation"]
            },

            new PhaseConfiguration
            {
                Id = ReplicatorPhases.Signing,
                DisplayName = "Signing",
                SupportsSequential = true,
                SupportsParallel = false,
                Actions = ["Signing"]
            }
        ],
        // Outcome definitions (base outcomes plus approval-specific ones)
        Outcomes =
        [
            new OutcomeConfiguration { Key = "Completed", DisplayValue = "Completed" },
            new OutcomeConfiguration { Key = "Cancelled", DisplayValue = "Cancelled" },
            new OutcomeConfiguration { Key = "Invalid", DisplayValue = "Invalid" },
            new OutcomeConfiguration { Key = "Approved", DisplayValue = "Approved" },
            new OutcomeConfiguration { Key = "Rejected", DisplayValue = "Rejected" }
        ],
        // Replicator phase configurations are now centralized here.
        ReplicatorPhaseConfigurations = new Dictionary<string, ReplicatorPhaseConfiguration>
        {
            { 
                ReplicatorPhases.Approval, new ReplicatorPhaseConfiguration
                {
                    ActivityData = (payload, workflowId) => payload.GetApprovalTasks(workflowId),
                    ReplicatorType = payload => payload.GetReplicatorType(ReplicatorPhases.Approval)
                }
            },
            { 
                ReplicatorPhases.Signing, new ReplicatorPhaseConfiguration
                {
                    ActivityData = (payload, workflowId) => payload.GetSigningTasks(workflowId),
                    ReplicatorType = payload => payload.GetReplicatorType(ReplicatorPhases.Signing)
                }
            }
        }
    };

    //public WorkflowDefinition GetDefinition()
    //{
    //    // Here you might build and return a client-friendly definition.
    //    return new WorkflowDefinition();
    //}
}
