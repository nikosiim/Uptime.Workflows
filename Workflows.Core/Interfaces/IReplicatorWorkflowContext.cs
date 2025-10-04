namespace Workflows.Core.Interfaces;

public interface IReplicatorWorkflowContext : IWorkflowContext
{
    Dictionary<string, ReplicatorState> ReplicatorStates { get; set; }
}