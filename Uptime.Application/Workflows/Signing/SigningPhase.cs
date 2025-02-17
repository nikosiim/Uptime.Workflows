using Uptime.Domain.Common;

namespace Uptime.Application.Workflows.Signing;


public sealed class SigningPhase : WorkflowPhase
{
    private SigningPhase(string value) : base(value) { }

    public static readonly SigningPhase Signing  = new("Signing");

    public static WorkflowPhase FromStringExtended(string value)
    {
        return value switch
        {
            "Signing"  => Signing,
            _ => FromString(value)
        };
    }
}