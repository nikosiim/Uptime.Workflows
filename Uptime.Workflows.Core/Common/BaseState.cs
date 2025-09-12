using System.Diagnostics.CodeAnalysis;

namespace Uptime.Workflows.Core.Common
{
    [SuppressMessage("Design", "NS1000:Seal class 'BaseState' or implement 'IEqualityComparer<T>' instead.", 
        Justification = "BaseState is abstract and only intended to be inherited by sealed classes (such as ExtendedState).")]
    public abstract class BaseState(string value) : IEquatable<BaseState>
    {
        public string Value { get; } = value;

        public static readonly BaseState NotStarted = new GenericWorkflowPhase("NotStarted");
        public static readonly BaseState InProgress = new GenericWorkflowPhase("InProgress");
        public static readonly BaseState Completed  = new GenericWorkflowPhase("Completed");
        public static readonly BaseState Cancelled  = new GenericWorkflowPhase("Cancelled");
        public static readonly BaseState Invalid    = new GenericWorkflowPhase("Invalid");

        public static BaseState FromString(string value)
        {
            return value switch
            {
                "NotStarted" => NotStarted,
                "InProgress" => InProgress,
                "Completed"  => Completed,
                "Cancelled"  => Cancelled,
                "Invalid"    => Invalid,
                _            => new GenericWorkflowPhase(value)
            };
        }

        public bool Equals(BaseState? other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) || string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj) => Equals(obj as BaseState);

        public override int GetHashCode() => Value.GetHashCode();
    }

    internal sealed class GenericWorkflowPhase(string value) : BaseState(value);
}
