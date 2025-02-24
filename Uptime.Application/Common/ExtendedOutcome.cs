using Uptime.Domain.Common;

namespace Uptime.Application.Common;

public sealed class ExtendedOutcome : WorkflowOutcome
{
    private ExtendedOutcome(string value) : base(value) { }

    public static readonly ExtendedOutcome Approved  = new("Approved");
    public static readonly ExtendedOutcome Rejected  = new("Rejected");
    public static readonly ExtendedOutcome Signed  = new("Signed");

    public static WorkflowOutcome FromStringExtended(string value)
    {
        return value switch
        {
            "Approved" => Approved,
            "Rejected" => Rejected,
            "Signed" => Signed,
            _ => FromString(value)
        };
    }
}