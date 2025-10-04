using Workflows.Core;
using Workflows.Core.Interfaces;
using static ApprovalWorkflow.Constants;

namespace ApprovalWorkflow;

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
        ]
    };
}