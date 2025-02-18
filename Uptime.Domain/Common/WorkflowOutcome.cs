namespace Uptime.Domain.Common;

public abstract class WorkflowOutcome(string? value)
{
    public string? Value { get; } = value;

    public static readonly WorkflowOutcome None = new GenericWorkflowOutcome(string.Empty);
    public static readonly WorkflowOutcome Completed  = new GenericWorkflowOutcome("Completed");
    public static readonly WorkflowOutcome Cancelled  = new GenericWorkflowOutcome("Cancelled");
    public static readonly WorkflowOutcome Invalid  = new GenericWorkflowOutcome("Invalid");
    
    public static WorkflowOutcome FromString(string? value)
    {
        return value switch
        {
            "" => None,
            "Completed" => Completed,
            "Cancelled" => Cancelled,
            "Invalid" => Invalid,
            _ => new GenericWorkflowOutcome(value)
        };
    }
}

internal sealed class GenericWorkflowOutcome(string? value) : WorkflowOutcome(value);