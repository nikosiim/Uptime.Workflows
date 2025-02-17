namespace Uptime.Domain.Common;

public abstract class WorkflowPhase(string value)
{
    public string Value { get; } = value;

    public static readonly WorkflowPhase NotStarted = new GenericWorkflowPhase("NotStarted");
    public static readonly WorkflowPhase InProgress = new GenericWorkflowPhase("InProgress");
    public static readonly WorkflowPhase Completed  = new GenericWorkflowPhase("Completed");
    public static readonly WorkflowPhase Cancelled  = new GenericWorkflowPhase("Cancelled");
    public static readonly WorkflowPhase Invalid  = new GenericWorkflowPhase("Invalid");
    
    public static WorkflowPhase FromString(string value)
    {
        return value switch
        {
            "NotStarted" => NotStarted,
            "InProgress" => InProgress,
            "Completed" => Completed,
            "Cancelled" => Cancelled,
            "Invalid" => Invalid,
            _ => new GenericWorkflowPhase(value)
        };
    }
}

internal sealed class GenericWorkflowPhase(string value) : WorkflowPhase(value);