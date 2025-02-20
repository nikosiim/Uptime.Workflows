using Uptime.Domain.Common;

namespace Uptime.Application.Workflows.Signing;

public sealed class SigningOutcome : WorkflowOutcome
{
    private SigningOutcome(string value) : base(value) { }

    public static readonly SigningOutcome Signed  = new("Signed");

    public static WorkflowOutcome FromStringExtended(string value)
    {
        return value switch
        {
            "Signed" => Signed,
            _ => FromString(value)
        };
    }
}