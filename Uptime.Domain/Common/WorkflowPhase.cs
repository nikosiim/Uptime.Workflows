namespace Uptime.Domain.Common;

public abstract class WorkflowPhase(string value) : IEquatable<WorkflowPhase>
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
    public bool Equals(WorkflowPhase? other)
    {
        if (other is null)
            return false;
        return ReferenceEquals(this, other) || string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as WorkflowPhase);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(WorkflowPhase? left, WorkflowPhase? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(WorkflowPhase? left, WorkflowPhase? right) => !(left == right);
}

internal sealed class GenericWorkflowPhase(string value) : WorkflowPhase(value);