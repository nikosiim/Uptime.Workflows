using Uptime.Workflows.Core.Common;

namespace SigningWorkflow;

internal sealed class ExtendedState : BaseState
{
    private ExtendedState(string value) : base(value) { }

    public static readonly ExtendedState Signing  = new("Signing");

    public static BaseState FromStringExtended(string value)
    {
        return value switch
        {
            "Signing"  => Signing,
            _ => FromString(value)
        };
    }
}