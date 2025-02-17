using Uptime.Domain.Common;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalOutcome : WorkflowOutcome
{
    private ApprovalOutcome(string value) : base(value) { }

    public static readonly ApprovalOutcome Approved  = new("Approved");
    public static readonly ApprovalOutcome Rejected  = new("Rejected");

    public static WorkflowOutcome FromStringExtended(string value)
    {
        return value switch
        {
            "Approved" => Approved,
            "Rejected" => Rejected,
            _ => FromString(value)
        };
    }
}