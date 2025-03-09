using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Choices;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalWorkflowDefinition : IWorkflowDefinition
{
    public Type Type => typeof(ApprovalWorkflow);
    public Type ContextType => typeof(ApprovalWorkflowContext);

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
                PhaseId = ExtendedState.Approval.Value,
                UpdateEnabled = true,
                SupportsSequential = true,
                SupportsParallel = true,
                Actions = [ButtonAction.Approval, ButtonAction.Rejection, ButtonAction.Delegation, ButtonAction.Cancellation]
            },
            new PhaseActivity
            {
                PhaseId = ExtendedState.Signing.Value,
                UpdateEnabled = false,
                SupportsSequential = true,
                SupportsParallel = false,
                Actions = [ButtonAction.Signing, ButtonAction.Rejection]
            }
        ],
        PhaseConfigurations = new Dictionary<string, ReplicatorPhaseConfiguration>
        {
            {
                ExtendedState.Approval.Value, new ReplicatorPhaseConfiguration
                {
                    ActivityData = (payload, workflowId) => payload.GetApprovalTasks(workflowId),
                    ReplicatorType = payload => payload.GetReplicatorType(ExtendedState.Approval.Value)
                }
            },
            {
                ExtendedState.Signing.Value, new ReplicatorPhaseConfiguration
                {
                    ActivityData = (payload, workflowId) => payload.GetSigningTasks(workflowId),
                    ReplicatorType = payload => payload.GetReplicatorType(ExtendedState.Signing.Value)
                }
            }
        }
    };
}