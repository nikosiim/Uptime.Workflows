using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalWorkflowDefinition : IWorkflowDefinition
{
    public Type Type => typeof(ApprovalWorkflow);
    public string Name => Type.Name;
    public string DisplayName => "Kinnitamise töövoog";
    public string Id => "16778969-6d4c-4367-9106-1b0ae4a4594f";

    public WorkflowDefinition GetDefinition()
    {
        return new WorkflowDefinition
        {
            Id = Id,
            Name = Name,
            DisplayName = DisplayName,
            ReplicatorActivities = ReplicatorConfiguration.PhaseActivities
        };
    }

    public ReplicatorConfiguration ReplicatorConfiguration { get; } = new()
    {
        PhaseActivities = 
        [
            new PhaseActivity
            {
                PhaseId = ReplicatorPhases.Approval,
                SupportsSequential = true,
                SupportsParallel = true,
                Actions = ["Approval", "Rejection", "Forward", "Cancellation"]
            },
            new PhaseActivity
            {
                PhaseId = ReplicatorPhases.Signing,
                SupportsSequential = true,
                SupportsParallel = false,
                Actions = ["Signing"]
            }
        ],
        PhaseConfigurations = new Dictionary<string, ReplicatorPhaseConfiguration>
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
}