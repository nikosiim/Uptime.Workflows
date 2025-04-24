using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IReplicatorWorkflowContext : IWorkflowContext
{
    Dictionary<string, ReplicatorState> ReplicatorStates { get; set; }
}