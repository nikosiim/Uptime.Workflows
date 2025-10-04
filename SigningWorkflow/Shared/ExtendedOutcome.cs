using Workflows.Core;

namespace SigningWorkflow;

internal sealed class ExtendedOutcome : WorkflowOutcome
{
    private ExtendedOutcome(string value) : base(value) { }

    public static readonly ExtendedOutcome Rejected  = new("Rejected");
    public static readonly ExtendedOutcome Signed  = new("Signed");

    public static WorkflowOutcome FromStringExtended(string value)
    {
        return value switch
        {
            "Rejected" => Rejected,
            "Signed" => Signed,
            _ => FromString(value)
        };
    }
}