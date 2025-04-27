namespace Uptime.Workflows.Core;

public interface IReplicatorWorkflowContext : IWorkflowContext
{
    Dictionary<string, ReplicatorState> ReplicatorStates { get; set; }
}