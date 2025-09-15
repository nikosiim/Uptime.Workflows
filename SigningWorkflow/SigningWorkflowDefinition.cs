using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Interfaces;
using static SigningWorkflow.Constants;

namespace SigningWorkflow;

// TODO: consider declare possible input dictionary keys
public sealed class SigningWorkflowDefinition : IWorkflowDefinition
{
    public Type Type => typeof(SigningWorkflow);
    public Type ContextType => typeof(BaseWorkflowContext);
    public string Name => Type.Name;
    public string DisplayName => "Allkirjastamise töövoog";
    public string Id => "52C90EB4-7F3D-4A1D-B469-3F3B064F76D7";

    public WorkflowDefinition GetDefinition()
    {
        return new WorkflowDefinition
        {
            Id = Id,
            Name = Name,
            DisplayName = DisplayName,
            Actions = [ButtonAction.Signing, ButtonAction.Rejection]
        };
    }

    public ReplicatorConfiguration? ReplicatorConfiguration => null;
}