using Uptime.Workflows.Core.Common;

namespace Uptime.Application.Common;

public sealed class ExtendedState : BaseState
{
    private ExtendedState(string value) : base(value) { }

    public static readonly ExtendedState Approval = new("Approval");
    public static readonly ExtendedState Signing  = new("Signing");

    public static BaseState FromStringExtended(string value)
    {
        return value switch
        {
            "Approval" => Approval,
            "Signing"  => Signing,
            _ => FromString(value)
        };
    }
}