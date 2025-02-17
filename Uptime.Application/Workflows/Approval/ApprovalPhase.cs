using Uptime.Domain.Common;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalPhase : WorkflowPhase
{
    private ApprovalPhase(string value) : base(value) { }

    public static readonly ApprovalPhase Approval = new("Approval");
    public static readonly ApprovalPhase Signing  = new("Signing");

    public static WorkflowPhase FromStringExtended(string value)
    {
        return value switch
        {
            "Approval" => Approval,
            "Signing"  => Signing,
            _ => FromString(value)
        };
    }
}